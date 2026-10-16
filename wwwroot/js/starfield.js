// starfield.js — a soft, slowly-wandering animated starfield used behind
// several sections of the site (hero, news, article pages, footer).
// Usage: <canvas class="starfield-canvas" data-count="24" data-colors="#49b9ff,#8fd8ff,#6fc3ff"></canvas>
// Optional: data-avoid-pixels="#someCanvas" — stars bounce off actual bright
// pixels drawn on that other canvas (e.g. a logo), instead of a fixed box.
(function () {
    'use strict';

    function hexToRgba(color, alpha) {
        if (color.indexOf('rgb') === 0) return color;
        var hex = color.replace('#', '');
        if (hex.length === 3) hex = hex.split('').map(function (c) { return c + c; }).join('');
        var r = parseInt(hex.substring(0, 2), 16);
        var g = parseInt(hex.substring(2, 4), 16);
        var b = parseInt(hex.substring(4, 6), 16);
        return 'rgba(' + r + ',' + g + ',' + b + ',' + alpha + ')';
    }

    function initStarfield(canvas) {
        var ctx = canvas.getContext('2d');
        var dpr = Math.min(window.devicePixelRatio || 1, 2);
        var count = parseInt(canvas.dataset.count, 10) || 22;
        var colors = (canvas.dataset.colors || '#49b9ff,#8fd8ff,#6fc3ff').split(',');
        var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        var stars = [];
        var w = 0, h = 0, running = true, rafId = null;

        var avoidCanvasSel = canvas.dataset.avoidPixels; // e.g. "#heroCanvas"
        var avoidCanvas = avoidCanvasSel ? document.querySelector(avoidCanvasSel) : null;
        var avoidCtx = avoidCanvas ? avoidCanvas.getContext('2d') : null;

        function isOnLogo(pageX, pageY) {
            if (!avoidCanvas || !avoidCtx) return false;
            var rect = avoidCanvas.getBoundingClientRect();
            if (pageX < rect.left || pageX > rect.right || pageY < rect.top || pageY > rect.bottom) return false;
            var px = Math.round((pageX - rect.left) * (avoidCanvas.width / rect.width));
            var py = Math.round((pageY - rect.top) * (avoidCanvas.height / rect.height));
            if (px < 0 || py < 0 || px >= avoidCanvas.width || py >= avoidCanvas.height) return false;
            try {
                var d = avoidCtx.getImageData(px, py, 1, 1).data;
                return (d[0] + d[1] + d[2]) > 40; // above this = actual logo pixel, not black background
            } catch (e) {
                return false; // fail-safe: never block stars if reading fails
            }
        }

        function resize() {
            var rect = canvas.getBoundingClientRect();
            w = Math.max(1, rect.width);
            h = Math.max(1, rect.height);
            canvas.width = Math.round(w * dpr);
            canvas.height = Math.round(h * dpr);
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        }

        function makeStar() {
            var tier = Math.random();
            var radius = tier > 0.92 ? (2.6 + Math.random() * 1.6)
                : tier > 0.65 ? (1.6 + Math.random() * 1.1)
                    : (0.7 + Math.random() * 0.9);
            var angle = Math.random() * Math.PI * 2;
            var speed = 0.0035 + Math.random() * 0.012; // slow, gentle drift
            return {
                x: Math.random() * w,
                y: Math.random() * h,
                r: radius,
                color: colors[(Math.random() * colors.length) | 0],
                baseAlpha: 0.45 + Math.random() * 0.5,
                vx: Math.cos(angle) * speed,
                vy: Math.sin(angle) * speed,
                twinkleSpeed: 0.0005 + Math.random() * 0.0011,
                twinklePhase: Math.random() * Math.PI * 2
            };
        }

        function seed() {
            stars = [];
            for (var i = 0; i < count; i++) stars.push(makeStar());
        }

        function drawStar(s, alpha) {
            var glowR = s.r * 4.5;
            var grad = ctx.createRadialGradient(s.x, s.y, 0, s.x, s.y, glowR);
            grad.addColorStop(0, hexToRgba(s.color, alpha));
            grad.addColorStop(1, hexToRgba(s.color, 0));
            ctx.fillStyle = grad;
            ctx.beginPath();
            ctx.arc(s.x, s.y, glowR, 0, Math.PI * 2);
            ctx.fill();

            ctx.fillStyle = hexToRgba(s.color, Math.min(1, alpha + 0.3));
            ctx.beginPath();
            ctx.arc(s.x, s.y, s.r, 0, Math.PI * 2);
            ctx.fill();
        }

        function frame(t) {
            if (!running) { rafId = null; return; }
            var canvasRect = canvas.getBoundingClientRect();
            ctx.clearRect(0, 0, w, h);
            for (var i = 0; i < stars.length; i++) {
                var s = stars[i];
                if (!reduceMotion) {
                    s.x += s.vx * 16;
                    s.y += s.vy * 16;
                    if (s.x < -12) s.x = w + 12;
                    if (s.x > w + 12) s.x = -12;
                    if (s.y < -12) s.y = h + 12;
                    if (s.y > h + 12) s.y = -12;

                    var pageX = canvasRect.left + s.x;
                    var pageY = canvasRect.top + s.y;
                    if (isOnLogo(pageX, pageY)) {
                        s.x -= s.vx * 16;
                        s.y -= s.vy * 16;
                        s.vx = -s.vx;
                        s.vy = -s.vy;
                    }
                }
                var twinkle = reduceMotion ? 1 : (0.62 + 0.38 * Math.sin(t * s.twinkleSpeed + s.twinklePhase));
                drawStar(s, s.baseAlpha * twinkle);
            }
            rafId = requestAnimationFrame(frame);
        }

        resize();
        seed();
        rafId = requestAnimationFrame(frame);

        var resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                resize();
                seed();
            }, 150);
        });

        if ('IntersectionObserver' in window) {
            var io = new IntersectionObserver(function (entries) {
                entries.forEach(function (entry) {
                    running = entry.isIntersecting;
                    if (running && !rafId) rafId = requestAnimationFrame(frame);
                });
            }, { threshold: 0 });
            io.observe(canvas);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var canvases = document.querySelectorAll('.starfield-canvas');
        canvases.forEach(function (c) { initStarfield(c); });
    });
})();