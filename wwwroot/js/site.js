// Theme toggle (home page only) - View Transitions API + animated clip-path circle.
// The #themeToggle button is rendered only on the home page, so on every other
// page this block exits immediately. Dark is the default; light = html.light-theme.
(function () {
    var root = document.documentElement;
    var themeToggle = document.getElementById('themeToggle');
    if (!themeToggle) return;

    var DURATION = 600;
    var busy = false;

    function setTheme(light) {
        root.classList.toggle('light-theme', light);
        try {
            localStorage.setItem('theme', light ? 'light' : 'dark');
        } catch (e) {}
    }

    themeToggle.addEventListener('click', function (e) {
        e.preventDefault();
        if (busy) return;

        // Direction comes from the current state: going dark = currently light.
        var goingDark = root.classList.contains('light-theme');
        var goingLight = !goingDark;

        var reduceMotion = window.matchMedia &&
            window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        if (typeof document.startViewTransition !== 'function' || reduceMotion) {
            setTheme(goingLight);
            return;
        }

        // Wave origin = centre of the button; radius = distance to the farthest viewport corner.
        var rect = themeToggle.getBoundingClientRect();
        var x = rect.left + rect.width / 2;
        var y = rect.top + rect.height / 2;
        var radius = Math.hypot(
            Math.max(x, window.innerWidth - x),
            Math.max(y, window.innerHeight - y)
        );

        busy = true;
        var animation = null;

        // Going light: the old (dark) snapshot must sit on top and shrink away.
        if (goingLight) root.classList.add('to-light');

        function cleanup() {
            if (animation) {
                try { animation.cancel(); } catch (err) {}
                animation = null;
            }
            root.classList.remove('to-light');
            busy = false;
        }

        var transition;
        try {
            transition = document.startViewTransition(function () {
                setTheme(goingLight);
            });
        } catch (err) {
            setTheme(goingLight);
            cleanup();
            return;
        }

        transition.ready.then(function () {
            var from = 'circle(0px at ' + x + 'px ' + y + 'px)';
            var to = 'circle(' + radius + 'px at ' + x + 'px ' + y + 'px)';

            animation = root.animate(
                { clipPath: goingLight ? [to, from] : [from, to] },
                {
                    duration: DURATION,
                    easing: 'ease-in-out',
                    fill: 'forwards',
                    pseudoElement: goingLight
                        ? '::view-transition-old(root)'
                        : '::view-transition-new(root)'
                }
            );
        }).catch(function () {
            // Transition was skipped (e.g. hidden tab): the DOM update still ran,
            // and transition.finished below resets the state.
        });

        // Only reset once the transition has really finished.
        transition.finished.then(cleanup, cleanup);
    });
})();

// Header transparency on scroll
(function () {
    var header = document.querySelector('.site-header');
    if (!header) return;

    var threshold = 80;

    function updateHeader() {
        if (window.scrollY < threshold) {
            header.classList.add('is-transparent');
        } else {
            header.classList.remove('is-transparent');
        }
    }

    updateHeader();
    window.addEventListener('scroll', updateHeader, { passive: true });
})();

// Subtle parallax tilt for glass cards (purely visual, no functional change)
(function () {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    var cards = document.querySelectorAll('.news-card, .service-card, .feature-item');
    if (!cards.length) return;

    cards.forEach(function (card) {
        card.addEventListener('mousemove', function (e) {
            var rect = card.getBoundingClientRect();
            var x = (e.clientX - rect.left) / rect.width - 0.5;
            var y = (e.clientY - rect.top) / rect.height - 0.5;
            card.style.transform = 'translateY(-6px) perspective(900px) rotateX(' + (y * -3) + 'deg) rotateY(' + (x * 3) + 'deg)';
        });
        card.addEventListener('mouseleave', function () {
            card.style.transform = '';
        });
    });
})();
