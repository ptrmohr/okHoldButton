/**
 * okHoldButton HTML — browser ES module, no dependencies
 * Copyright (c) 2026 OnlineKraft UG — MIT license
 *
 * Complete the hold, then release to confirm.
 * A click, early release, moving away, or losing focus does not confirm.
 */
export class OkHoldButton {
  static defaults = Object.freeze({
    holdDuration: 5000,
    text: "GEDRÜCKT HALTEN",
    subText: "Zum Bestätigen gedrückt halten",
    releaseText: "JETZT LOSLASSEN",
    hintText: "Bitte länger gedrückt halten",
    successText: "BESTÄTIGT",
    fillColor: "#c4d7e4",
    accentColor: "#3e6377",
    textColor: "#2f4d5e",
    disabled: false,
    onComplete: null,
    onCancel: null
  });

  constructor(target, options = {}) {
    const button = typeof target === "string" ? document.querySelector(target) : target;
    if (!(button instanceof HTMLButtonElement)) {
      throw new TypeError("OkHoldButton needs an HTML <button> or a selector for one.");
    }
    if (button.dataset.okHoldButton === "active") {
      throw new Error("This button already has an OkHoldButton instance.");
    }
    this.element = button;
    this._savedHTML = button.innerHTML;
    this._savedDisabled = button.disabled;
    this._savedType = button.getAttribute("type");
    this._settings = { ...OkHoldButton.defaults, ...options };
    this._checkDuration(this._settings.holdDuration);
    this._state = "idle";
    this._source = null;
    this._pointerId = null;
    this._progress = 0;
    this._startedAt = 0;
    this._frame = 0;
    this._count = 0;
    this._destroyed = false;
    this._listeners = [];

    button.dataset.okHoldButton = "active";
    button.classList.add("okhb");
    button.type = "button";
    button.replaceChildren();
    this._fill = document.createElement("span");
    this._fill.className = "okhb__fill";
    this._fill.setAttribute("aria-hidden", "true");
    const content = document.createElement("span");
    content.className = "okhb__content";
    this._label = document.createElement("span");
    this._label.className = "okhb__label";
    this._subtitle = document.createElement("span");
    this._subtitle.className = "okhb__subtitle";
    content.append(this._label, this._subtitle);
    button.append(this._fill, content);
    this._paintOptions();
    this._resetVisual();
    button.disabled = Boolean(this._settings.disabled);

    this._on(button, "pointerdown", e => {
      if (this.disabled || (e.pointerType === "mouse" && e.button !== 0)) return;
      if (this._isHolding()) return;
      e.preventDefault();
      this._start("pointer", e.pointerId);
      try { button.setPointerCapture(e.pointerId); } catch (_) {}
    });
    this._on(button, "pointermove", e => {
      if (this._source === "pointer" && e.pointerId === this._pointerId && !this._inside(e)) {
        this.cancel("pointer-left");
      }
    });
    this._on(button, "pointerup", e => {
      if (this._source !== "pointer" || e.pointerId !== this._pointerId) return;
      if (this._inside(e)) this._release();
      else this.cancel("pointer-left");
    });
    this._on(button, "pointercancel", e => {
      if (this._source === "pointer" && e.pointerId === this._pointerId) this.cancel("pointercancel");
    });
    this._on(button, "lostpointercapture", () => {
      if (this._source === "pointer") this.cancel("lost-capture");
    });
    // The browser's ordinary click must never execute a protected action.
    this._on(button, "click", e => {
      e.preventDefault();
      e.stopImmediatePropagation();
    }, true);
    this._on(button, "keydown", e => {
      if (e.code === "Enter") { e.preventDefault(); return; }
      if (e.code !== "Space") return;
      e.preventDefault();
      if (!e.repeat && !this.disabled && !this._isHolding()) this._start("keyboard");
    });
    this._on(button, "keyup", e => {
      if (e.code === "Enter") { e.preventDefault(); return; }
      if (e.code !== "Space") return;
      e.preventDefault();
      if (this._source === "keyboard") this._release();
    });
    this._on(button, "contextmenu", e => e.preventDefault());
    this._on(button, "blur", () => {
      if (this._source === "keyboard") this.cancel("blur");
    });
    this._on(window, "blur", () => this.cancel("window-blur"));
    this._on(document, "visibilitychange", () => {
      if (document.hidden) this.cancel("page-hidden");
    });
  }

  get holdDuration() { return this._settings.holdDuration; }
  set holdDuration(value) {
    this._checkDuration(value);
    this.cancel("reconfigured");
    this._settings.holdDuration = Number(value);
    this._resetVisual();
  }
  get text() { return this._settings.text; }
  set text(v) { this._settings.text = String(v); if (!this._isHolding()) this._resetVisual(); }
  get subText() { return this._settings.subText; }
  set subText(v) { this._settings.subText = String(v); if (!this._isHolding()) this._resetVisual(); }
  get releaseText() { return this._settings.releaseText; }
  set releaseText(v) { this._settings.releaseText = String(v); }
  get hintText() { return this._settings.hintText; }
  set hintText(v) { this._settings.hintText = String(v); }
  get successText() { return this._settings.successText; }
  set successText(v) { this._settings.successText = String(v); }
  get fillColor() { return this._settings.fillColor; }
  set fillColor(v) { this._settings.fillColor = String(v); this._paintOptions(); }
  get accentColor() { return this._settings.accentColor; }
  set accentColor(v) { this._settings.accentColor = String(v); this._paintOptions(); }
  get textColor() { return this._settings.textColor; }
  set textColor(v) { this._settings.textColor = String(v); this._paintOptions(); }
  get disabled() { return Boolean(this._settings.disabled); }
  set disabled(v) {
    if (v) this.cancel("disabled");
    this._settings.disabled = Boolean(v);
    this.element.disabled = Boolean(v);
  }
  get onComplete() { return this._settings.onComplete; }
  set onComplete(fn) { this._checkCallback(fn); this._settings.onComplete = fn; }
  get onCancel() { return this._settings.onCancel; }
  set onCancel(fn) { this._checkCallback(fn); this._settings.onCancel = fn; }
  get progress() { return this._progress; }
  get state() { return this._state; }
  get completionCount() { return this._count; }

  reset() {
    this.cancel("reset");
    this._state = "idle";
    this._resetVisual();
  }

  cancel(reason = "cancelled") {
    if (!this._isHolding()) return;
    const detail = this._detail({ reason, progress: this._progress });
    this._stop();
    this._state = "idle";
    this._source = null;
    this._pointerId = null;
    this._resetVisual();
    this._emit("holdcancelled", detail);
    this._settings.onCancel?.(detail);
  }

  destroy() {
    if (this._destroyed) return;
    this.cancel("destroyed");
    this._stop();
    for (const [node, name, fn, opts] of this._listeners) node.removeEventListener(name, fn, opts);
    this._listeners = [];
    this.element.innerHTML = this._savedHTML;
    this.element.disabled = this._savedDisabled;
    if (this._savedType === null) this.element.removeAttribute("type");
    else this.element.setAttribute("type", this._savedType);
    this.element.classList.remove("okhb", "okhb--holding", "okhb--ready", "okhb--success");
    delete this.element.dataset.okHoldButton;
    for (const prop of ["--okhb-fill", "--okhb-accent", "--okhb-text"]) {
      this.element.style.removeProperty(prop);
    }
    this._destroyed = true;
  }

  _checkDuration(ms) {
    if (!Number.isFinite(Number(ms)) || Number(ms) < 250 || Number(ms) > 60000) {
      throw new RangeError("holdDuration must be 250–60000 milliseconds.");
    }
  }
  _checkCallback(fn) {
    if (fn !== null && typeof fn !== "function") throw new TypeError("Callback must be a function or null.");
  }
  _on(node, event, fn, opts) {
    node.addEventListener(event, fn, opts);
    this._listeners.push([node, event, fn, opts]);
  }
  _paintOptions() {
    this.element.style.setProperty("--okhb-fill", this.fillColor);
    this.element.style.setProperty("--okhb-accent", this.accentColor);
    this.element.style.setProperty("--okhb-text", this.textColor);
  }
  _resetVisual() {
    this._progress = 0;
    this.element.classList.remove("okhb--holding", "okhb--ready", "okhb--success");
    this._fill.style.width = "0%";
    this._label.textContent = this.text;
    this._subtitle.textContent = this.subText;
    this.element.setAttribute("aria-label", this.text);
  }
  _isHolding() { return this._state === "holding" || this._state === "ready"; }
  _inside(e) {
    const r = this.element.getBoundingClientRect();
    return e.clientX >= r.left && e.clientX <= r.right && e.clientY >= r.top && e.clientY <= r.bottom;
  }
  _detail(extra = {}) {
    return { durationMs: this.holdDuration, elapsedMs: Math.max(0, performance.now() - this._startedAt), source: this._source, ...extra };
  }
  _emit(name, detail) {
    this.element.dispatchEvent(new CustomEvent(name, { bubbles: true, detail }));
  }
  _stop() { cancelAnimationFrame(this._frame); this._frame = 0; }
  _seconds(ms) { return (Math.max(0, ms) / 1000).toFixed(1); }

  _start(source, id = null) {
    if (this._destroyed || this.disabled || this._isHolding()) return;
    this._stop();
    this._source = source;
    this._pointerId = id;
    this._state = "holding";
    this._startedAt = performance.now();
    this._progress = 0;
    this.element.classList.remove("okhb--ready", "okhb--success");
    this.element.classList.add("okhb--holding");
    this._fill.style.width = "0%";
    this._label.textContent = this.text;
    this._subtitle.textContent = this._seconds(this.holdDuration) + " s halten";
    this._emit("holdstart", this._detail({ progress: 0 }));
    this._frame = requestAnimationFrame(now => this._tick(now));
  }

  _tick(now) {
    if (this._state !== "holding") return;
    const elapsed = Math.max(0, now - this._startedAt);
    this._progress = Math.min(1, elapsed / this.holdDuration);
    this._fill.style.width = (this._progress * 100).toFixed(2) + "%";
    this._subtitle.textContent = "Noch " + this._seconds(this.holdDuration - elapsed) + " s";
    this._emit("holdprogress", this._detail({ progress: this._progress, elapsedMs: elapsed }));
    if (this._progress === 1) {
      this._state = "ready";
      this.element.classList.remove("okhb--holding");
      this.element.classList.add("okhb--ready");
      this._label.textContent = this.releaseText;
      this._subtitle.textContent = "Zum Bestätigen loslassen";
      this.element.setAttribute("aria-label", this.releaseText);
      this._emit("holdready", this._detail({ progress: 1, elapsedMs: elapsed }));
      return;
    }
    this._frame = requestAnimationFrame(t => this._tick(t));
  }

  _release() {
    if (!this._isHolding()) return;
    const elapsed = Math.max(0, performance.now() - this._startedAt);
    const confirmed = elapsed >= this.holdDuration;
    const source = this._source;
    this._stop();
    this._source = null;
    this._pointerId = null;
    if (confirmed) {
      this._progress = 1;
      this._count += 1;
      this._state = "success";
      this.element.classList.remove("okhb--holding", "okhb--ready");
      this.element.classList.add("okhb--success");
      this._fill.style.width = "100%";
      this._label.textContent = this.successText;
      this._subtitle.textContent = "Aktion bestätigt";
      this.element.setAttribute("aria-label", this.successText);
      const detail = { durationMs: this.holdDuration, elapsedMs: elapsed, source, count: this._count };
      this._emit("holdcompleted", detail);
      this._settings.onComplete?.(detail);
    } else {
      const detail = { durationMs: this.holdDuration, elapsedMs: elapsed, source, reason: "released-early", progress: this._progress };
      this._state = "idle";
      this._resetVisual();
      this._emit("holdcancelled", detail);
      this._settings.onCancel?.(detail);
      if (elapsed < 400 && this.hintText) this._subtitle.textContent = this.hintText;
    }
  }
}

export default OkHoldButton;
