(function () {
    var header = document.querySelector('.site-header');
    if (!header) return;

    var threshold = 80; // بعد از چند پیکسل اسکرول، هدر توپُر بشه

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