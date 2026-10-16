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
