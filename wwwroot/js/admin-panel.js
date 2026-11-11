/* =========================================================
   admin-panel.js: behavior for Views/AdminPanel/Dashboard.cshtml (FRONT-END ONLY, nothing is sent to a server).
   Sections: 1 helpers | 2 toasts | 3 modals | 4 sidebar (rail, drawer, groups, tooltips) | 5 routing (?tab=)
             6 small widgets | 7 post EDITOR (rich text, validation, draft autosave, live preview)
   ========================================================= */
(function () {
    'use strict';

    var root = document.getElementById('adminZone');
    if (!root) return;

    /* ---------- 1. helpers ---------- */
    var $ = function (sel, ctx) { return (ctx || root).querySelector(sel); };
    var $$ = function (sel, ctx) { return Array.prototype.slice.call((ctx || root).querySelectorAll(sel)); };
    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var SVGNS = 'http://www.w3.org/2000/svg';

    function icon(id, cls) {
        var svg = document.createElementNS(SVGNS, 'svg');
        svg.setAttribute('class', cls || 'admin-ico');
        svg.setAttribute('aria-hidden', 'true');
        var use = document.createElementNS(SVGNS, 'use');
        use.setAttribute('href', '#' + id);
        svg.appendChild(use);
        return svg;
    }

    function toLatinDigits(s) {
        return String(s || '')
            .replace(/[\u06F0-\u06F9]/g, function (d) { return String(d.charCodeAt(0) - 0x06F0); })
            .replace(/[\u0660-\u0669]/g, function (d) { return String(d.charCodeAt(0) - 0x0660); });
    }

    function faNum(n) {
        try { return Number(n).toLocaleString('fa-IR'); } catch (e) { return String(n); }
    }

    function faTime(d) {
        try { return d.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' }); } catch (e) { return ''; }
    }

    function focusables(container) {
        return $$('a[href], button:not([disabled]), input:not([disabled]):not([type="hidden"]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"]), [contenteditable="true"]', container)
            .filter(function (el) {
                return el.getClientRects().length > 0 && window.getComputedStyle(el).visibility !== 'hidden';
            });
    }

    /* keep --admin-top equal to the real sticky header height */
    function syncTop() {
        var h = document.querySelector('.site-header');
        if (h) root.style.setProperty('--admin-top', Math.round(h.getBoundingClientRect().height) + 'px');
    }
    syncTop();
    window.addEventListener('resize', syncTop);

    /* Visible height of the screen (visualViewport follows the mobile address bar / toolbars exactly).
       --vv-h  = that height in px: the off-canvas drawer is sized with it (see admin-panel.css).
       --app-vh = 1% of it: fallback only where dvh is unsupported. */
    function syncVh() {
        var vv = window.visualViewport;
        var h = vv ? vv.height : window.innerHeight;
        var st = document.documentElement.style;
        st.setProperty('--app-vh', (h * 0.01) + 'px');
        st.setProperty('--vv-h', h + 'px');
    }
    syncVh();
    window.addEventListener('resize', syncVh);
    window.addEventListener('orientationchange', syncVh);
    if (window.visualViewport) {
        window.visualViewport.addEventListener('resize', syncVh);
    }
    if ('ResizeObserver' in window && document.querySelector('.site-header')) {
        new ResizeObserver(syncTop).observe(document.querySelector('.site-header'));
    }

    /* ---------- 2. toasts ---------- */
    var toastBox = $('#adminToasts');
    function toast(message, type, ms) {
        if (!toastBox) return;
        type = type || 'info';
        while (toastBox.children.length >= 3) toastBox.removeChild(toastBox.firstChild);
        var t = document.createElement('div');
        t.className = 'admin-toast admin-toast--' + type;
        t.setAttribute('role', type === 'error' ? 'alert' : 'status');
        t.appendChild(icon(type === 'success' ? 'ad-check' : 'ad-alert'));
        var span = document.createElement('span');
        span.textContent = message;
        t.appendChild(span);
        var close = document.createElement('button');
        close.type = 'button';
        close.className = 'admin-toast-close';
        close.setAttribute('aria-label', 'بستن پیام');
        close.appendChild(icon('ad-x'));
        t.appendChild(close);
        toastBox.appendChild(t);
        function remove() { if (t.parentNode) t.parentNode.removeChild(t); }
        close.addEventListener('click', remove);
        setTimeout(remove, ms || 5000);
    }

    /* ---------- 3. modals (focus trap + return focus) ---------- */
    var openModals = [];
    var shell = $('#adminShell');
    var sidebar = $('#adminSidebar');
    var backdrop = $('#adminBackdrop');
    var menuBtn = $('#adminMenuBtn');
    var closeBtn = $('#adminCloseBtn');
    var collapseBtn = $('#adminCollapseBtn');

    function updateScrollLock() {
        var lock = openModals.length > 0 || (shell && shell.classList.contains('is-drawer-open'));
        document.body.classList.toggle('admin-scroll-lock', lock);
    }

    function openModal(modal, opener) {
        if (!modal) return;
        modal.hidden = false;
        modal._opener = opener || document.activeElement;
        openModals.push(modal);
        updateScrollLock();
        var first = $('[data-autofocus]', modal) || $('[data-close-modal].admin-modal-close', modal) || focusables(modal)[0];
        if (first) first.focus();
    }

    function closeModal(modal) {
        if (!modal || modal.hidden) return;
        modal.hidden = true;
        openModals = openModals.filter(function (m) { return m !== modal; });
        updateScrollLock();
        if (modal._opener && typeof modal._opener.focus === 'function') modal._opener.focus();
    }

    var previewModal = $('#postPreviewModal');
    if (previewModal) root.appendChild(previewModal); // keep fixed overlays at the zone root

    root.addEventListener('click', function (e) {
        var closer = e.target.closest('[data-close-modal]');
        if (closer) closeModal(closer.closest('.admin-modal'));
    });

    var newsModal = $('#newsModal');
    var newNewsBtn = $('[data-action="new-news"]');
    if (newsModal && newNewsBtn) {
        newNewsBtn.addEventListener('click', function () { openModal(newsModal, newNewsBtn); });
    }

    /* ---------- 4. sidebar ---------- */
    var NAV_KEY = 'admin.sidebar.collapsed';
    var mqDrawer = window.matchMedia('(max-width: 1024px)');
    var nav = $('#adminNav');
    var tip = $('#adminTip');
    var groups = $$('.admin-nav-group', nav);
    var lastFocus = null;

    function isDrawerMode() { return mqDrawer.matches; }
    function isRail() { return !isDrawerMode() && shell.classList.contains('is-collapsed'); }
    function drawerOpen() { return shell.classList.contains('is-drawer-open'); }

    function groupBtn(g) { return $('.admin-nav-toggle', g); }

    /* Strict single-open accordion. ONE piece of state (openGroupId, mirrored in nav[data-open-group]) drives every
       group in every mode (expanded, rail, drawer). Mouse, keyboard, deep links, rail/expanded switches and the drawer
       all go through setOpenGroup(), so two groups can never be open. Not persisted: on load only the group that
       holds the active item opens (see showPanel). */
    var openGroupId = null;

    function applyOpenGroup() {
        groups.forEach(function (g) {
            var open = g.getAttribute('data-group') === openGroupId;
            g.classList.toggle('is-open', open);
            groupBtn(g).setAttribute('aria-expanded', open ? 'true' : 'false');
            var sub = $('.admin-sub', g);
            if (sub) { if (open) sub.removeAttribute('inert'); else sub.setAttribute('inert', ''); } // closed => not focusable
        });
        nav.setAttribute('data-open-group', openGroupId || '');
    }

    function setOpenGroup(id) {
        var known = id && groups.some(function (g) { return g.getAttribute('data-group') === id; });
        openGroupId = known ? id : null;
        applyOpenGroup();
    }

    function toggleGroup(id) { setOpenGroup(openGroupId === id ? null : id); }

    /* collapsed rail: the open group and the active item survive the switch because nothing here touches them */
    function setCollapsed(v, persist) {
        shell.classList.toggle('is-collapsed', v);
        collapseBtn.setAttribute('aria-pressed', v ? 'true' : 'false');
        var label = v ? 'باز کردن منو' : 'جمع کردن منو';
        collapseBtn.setAttribute('aria-label', label);
        collapseBtn.setAttribute('data-tip', label);
        hideTip();
        applyOpenGroup();
        if (persist) { try { localStorage.setItem(NAV_KEY, v ? '1' : '0'); } catch (e) { } }
    }
    collapseBtn.addEventListener('click', function () { setCollapsed(!shell.classList.contains('is-collapsed'), true); });

    groups.forEach(function (g) {
        groupBtn(g).addEventListener('click', function () { toggleGroup(g.getAttribute('data-group')); });
    });

    window.addEventListener('resize', hideTip);

    /* tooltips: a one-line label for icon-only items in the rail (hover + keyboard focus), shown to the left of the item.
       A floating element, because the nav can scroll and would clip a CSS tooltip. */
    function showTip(el) {
        if (!tip || !isRail()) return;
        tip.textContent = el.getAttribute('data-tip');
        tip.hidden = false;
        var r = el.getBoundingClientRect();
        var t = tip.getBoundingClientRect();
        var left = r.left - 8 - t.width;
        if (left < 8) left = r.right + 8;
        tip.style.left = left + 'px';
        tip.style.top = Math.round(r.top + r.height / 2 - t.height / 2) + 'px';
    }
    function hideTip() { if (tip) tip.hidden = true; }

    sidebar.addEventListener('mouseover', function (e) { var el = e.target.closest('[data-tip]'); if (el) showTip(el); });
    sidebar.addEventListener('mouseout', function (e) { if (e.target.closest('[data-tip]')) hideTip(); });
    sidebar.addEventListener('focusin', function (e) { var el = e.target.closest('[data-tip]'); if (el && e.target.matches(':focus-visible')) showTip(el); });
    sidebar.addEventListener('focusout', hideTip);
    sidebar.addEventListener('click', hideTip);
    nav.addEventListener('scroll', hideTip, { passive: true });

    /* logout: the row only opens the confirmation dialog; the dialog's "خروج" button submits #adminLogoutForm (form="adminLogoutForm") */
    var logoutModal = $('#logoutModal');
    var logoutBtn = $('[data-action="logout"]', sidebar);
    if (logoutModal && logoutBtn) {
        root.appendChild(logoutModal); // keep the fixed overlay at the zone root, outside the (transformed) drawer
        logoutBtn.addEventListener('click', function () { hideTip(); openModal(logoutModal, logoutBtn); });
    }

    /* Drawer fit (build tag: drawer-fit v4).
       In drawer mode the drawer is sized from its REAL on-screen top to the REAL bottom of the visible area
       (visualViewport = what the user actually sees, address bar shown or hidden), so the bottom area
       (user card + logout) can never end up below the screen. Inline styles win over any cached/other CSS. */
    function fitDrawer() {
        if (!sidebar) return;
        if (!mqDrawer.matches) {
            sidebar.style.removeProperty('height');
            sidebar.style.removeProperty('max-height');
            sidebar.style.removeProperty('bottom');
            return;
        }
        var vv = window.visualViewport;
        var bottom = vv ? vv.offsetTop + vv.height : window.innerHeight;
        var top = sidebar.getBoundingClientRect().top;
        var h = Math.floor(bottom - top);
        if (h < 200) h = 200;
        sidebar.style.setProperty('bottom', 'auto', 'important');
        sidebar.style.setProperty('max-height', 'none', 'important');
        sidebar.style.setProperty('height', h + 'px', 'important');
    }
    window.addEventListener('resize', fitDrawer);
    window.addEventListener('orientationchange', fitDrawer);
    if (window.visualViewport) {
        window.visualViewport.addEventListener('resize', fitDrawer);
        window.visualViewport.addEventListener('scroll', fitDrawer);
    }
    if ('ResizeObserver' in window && document.querySelector('.site-header')) {
        new ResizeObserver(fitDrawer).observe(document.querySelector('.site-header'));
    }

    fitDrawer();

    /* ?debug=1 shows a tiny readout (bottom-left) so a layout problem can be diagnosed from a phone screenshot */
    if (/[?&]debug=1(&|$)/.test(window.location.search)) {
        var dbg = document.createElement('div');
        dbg.style.cssText = 'position:fixed;left:6px;bottom:6px;z-index:99999;background:rgba(0,0,0,.8);color:#7CFC00;font:11px/1.5 monospace;padding:4px 7px;border-radius:6px;pointer-events:none;direction:ltr;white-space:pre';
        document.body.appendChild(dbg);
        setInterval(function () {
            var vv = window.visualViewport;
            var r = sidebar.getBoundingClientRect();
            dbg.textContent = 'drawer-fit v4\n' +
                'inner ' + window.innerHeight + '  vv ' + (vv ? Math.round(vv.height) : '-') + '  vvTop ' + (vv ? Math.round(vv.offsetTop) : '-') + '\n' +
                'drawer top ' + Math.round(r.top) + '  bottom ' + Math.round(r.bottom) + '  h ' + Math.round(r.height) + '\n' +
                'adminTop ' + getComputedStyle(root).getPropertyValue('--admin-top') + '  mode ' + (mqDrawer.matches ? 'drawer' : 'desktop') + '\n' +
                'foot bottom ' + Math.round(sidebar.querySelector('.admin-sidebar-foot').getBoundingClientRect().bottom) + '  screen ' + screen.height + '  outer ' + window.outerHeight + '  dpr ' + window.devicePixelRatio;
        }, 400);
    }

    /* off-canvas drawer (<= 1024px) */
    function openDrawer() {
        if (drawerOpen()) return;
        lastFocus = document.activeElement;
        shell.classList.add('is-drawer-open');
        fitDrawer();
        requestAnimationFrame(fitDrawer);
        backdrop.hidden = false;
        menuBtn.setAttribute('aria-expanded', 'true');
        sidebar.setAttribute('role', 'dialog');
        sidebar.setAttribute('aria-modal', 'true');
        sidebar.setAttribute('aria-label', 'منوی پنل مدیریت');
        updateScrollLock();
        setTimeout(function () { closeBtn.focus(); }, 30);
    }

    function closeDrawer(returnFocus) {
        if (!drawerOpen()) return;
        shell.classList.remove('is-drawer-open');
        backdrop.hidden = true;
        menuBtn.setAttribute('aria-expanded', 'false');
        sidebar.removeAttribute('role');
        sidebar.removeAttribute('aria-modal');
        sidebar.removeAttribute('aria-label');
        updateScrollLock();
        if (returnFocus !== false && lastFocus && typeof lastFocus.focus === 'function') lastFocus.focus();
    }

    menuBtn.addEventListener('click', openDrawer);
    closeBtn.addEventListener('click', function () { closeDrawer(true); });
    backdrop.addEventListener('click', function () { closeDrawer(true); });
    document.addEventListener('pointerdown', function (e) {
        if (drawerOpen() && !openModals.length && !sidebar.contains(e.target) && !menuBtn.contains(e.target)) closeDrawer(false);
    });
    document.addEventListener('focusin', function (e) {
        if (drawerOpen() && !sidebar.contains(e.target) && !openModals.length) closeBtn.focus();
    });

    function onMqChange() {
        fitDrawer();
        if (!isDrawerMode()) closeDrawer(false);
        hideTip();
        applyOpenGroup();
    }
    if (mqDrawer.addEventListener) mqDrawer.addEventListener('change', onMqChange); else mqDrawer.addListener(onMqChange);

    /* keyboard: Tab trap (drawer / modal) and Esc */
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Tab') {
            var container = null;
            if (openModals.length) container = $('.admin-modal-card', openModals[openModals.length - 1]);
            else if (drawerOpen()) container = sidebar;
            if (!container) return;
            var items = focusables(container);
            if (!items.length) return;
            var first = items[0], last = items[items.length - 1];
            if (e.shiftKey && (document.activeElement === first || !container.contains(document.activeElement))) { e.preventDefault(); last.focus(); }
            else if (!e.shiftKey && (document.activeElement === last || !container.contains(document.activeElement))) { e.preventDefault(); first.focus(); }
            return;
        }
        if (e.key !== 'Escape') return;
        if (openModals.length) { closeModal(openModals[openModals.length - 1]); return; }
        if (drawerOpen()) closeDrawer(true);
    });

    /* ---------- 5. routing: one panel per menu item, ?tab=<id>, legacy aliases ---------- */
    var panels = $$('.admin-panel');
    var navLinks = $$('[data-panel-link]', nav);
    var crumbGroup = $('#crumbGroup');
    var crumbPage = $('#crumbPage');
    var ALIASES = { news: 'news', comments: 'comments-all', users: 'users-all', settings: 'settings-general' };

    function resolvePanel(name) {
        name = String(name || '').trim();
        if (ALIASES[name]) name = ALIASES[name];
        return panels.some(function (p) { return p.dataset.panel === name; }) ? name : null;
    }

    function showPanel(name, opts) {
        opts = opts || {};
        var id = resolvePanel(name) || 'dashboard';
        panels.forEach(function (p) { p.classList.toggle('is-active', p.dataset.panel === id); });

        var current = null;
        navLinks.forEach(function (a) {
            var isCur = a.dataset.panelLink === id && !a.classList.contains('admin-brand');
            if (isCur) { a.setAttribute('aria-current', 'page'); current = a; } else a.removeAttribute('aria-current');
        });

        var curGroup = current ? current.closest('.admin-nav-group') : null;
        groups.forEach(function (g) { g.classList.toggle('is-current', g === curGroup); });
        if (curGroup) setOpenGroup(curGroup.getAttribute('data-group'));
        else if (opts.user) setOpenGroup(null);

        var label = current ? current.textContent.trim().replace(/\s+/g, ' ') : 'داشبورد';
        crumbPage.textContent = label;
        if (curGroup) { crumbGroup.textContent = groupBtn(curGroup).textContent.trim(); crumbGroup.hidden = false; } else crumbGroup.hidden = true;

        if (opts.writeUrl !== false) {
            try {
                var url = new URL(window.location.href);
                url.searchParams.set('tab', id);
                history.replaceState(null, '', url);
            } catch (err) { }
        }
        if (opts.user) {
            closeDrawer(false);
            window.scrollTo(0, 0);
        }
    }

    root.addEventListener('click', function (e) {
        var a = e.target.closest('a[data-panel-link]');
        if (!a || e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        e.preventDefault();
        showPanel(a.dataset.panelLink, { user: true });
    });

    /* initial state */
    setCollapsed(shell.classList.contains('is-collapsed'), false); // also runs applyOpenGroup(): everything starts collapsed
    var initialTab = null;
    try { initialTab = new URLSearchParams(window.location.search).get('tab'); } catch (err) { }
    showPanel(initialTab, { writeUrl: !!initialTab });

    /* ---------- 6. small widgets ---------- */
    root.addEventListener('click', function (e) {
        var btn = e.target.closest('.admin-filter-group button');
        if (!btn) return;
        var group = btn.closest('.admin-filter-group');
        $$('button', group).forEach(function (b) { b.classList.remove('is-active'); });
        btn.classList.add('is-active');
    });

    var bell = $('#adminBell');
    if (bell) bell.addEventListener('click', function () { toast('اعلان جدیدی وجود ندارد. (نمونه رابط کاربری)', 'info', 3500); });
    var gsearch = $('#adminGlobalSearch');
    if (gsearch) gsearch.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); toast('جستجوی سراسری بعد از اتصال بک‌اند فعال می‌شود.', 'info', 3500); }
    });

    /* =========================================================
       7. EDITOR: "افزودن پست" (no backend; nothing is uploaded or saved on a server)
       ========================================================= */
    (function initEditor() {
        var form = $('#postForm');
        if (!form) return;

        /* BACKEND-TODO: shown on a valid "انتشار". Delete this constant and the toast call below once the backend exists. */
        var UI_PREVIEW_NOTICE = 'فرم معتبر است، اما این فقط پیش‌نمایش رابط کاربری است و هنوز چیزی ذخیره یا منتشر نشده است.';

        var DRAFT_KEY = 'ariaprint.admin.postDraft.v1';
        var MAX_COVER = 2 * 1024 * 1024;
        var COVER_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
        var MAX_SUMMARY = 200;

        var f = function (name) { return form.elements[name]; };
        var rteArea = $('#rteArea');
        var contentInput = $('#f-Content');
        var statusEl = $('#edStatus');
        var banner = $('#draftBanner');
        var shown = {}; // fields whose error is currently allowed to show

        /* ----- Jalali "today" as 1405/08/01 (Latin digits) ----- */
        function todayJalali() {
            try {
                var parts = new Intl.DateTimeFormat('en-US-u-ca-persian-nu-latn', { year: 'numeric', month: '2-digit', day: '2-digit' }).formatToParts(new Date());
                var get = function (t) { var p = parts.filter(function (x) { return x.type === t; })[0]; return p ? p.value : ''; };
                var y = get('year').replace(/\D/g, ''), m = get('month').replace(/\D/g, ''), d = get('day').replace(/\D/g, '');
                if (y && m && d) return y + '/' + ('0' + m).slice(-2) + '/' + ('0' + d).slice(-2);
            } catch (e) { }
            return '';
        }
        f('ReleaseDate').value = todayJalali();

        /* ----- inline errors ----- */
        function ctl(name) {
            if (name === 'Content') return rteArea;
            if (name === 'PictureUrl') return f('PictureUrl');
            return f(name);
        }
        function setError(name, msg) {
            var err = $('#e-' + name);
            var c = ctl(name);
            if (!err || !c) return;
            err.textContent = msg || '';
            err.hidden = !msg;
            if (msg) c.setAttribute('aria-invalid', 'true'); else c.removeAttribute('aria-invalid');
            var mark = name === 'Content' ? $('#rte') : (name === 'PictureUrl' ? $('#coverDrop') : c);
            if (mark) mark.classList.toggle('is-invalid', !!msg);
        }

        /* ----- validation ----- */
        var DATE_RE = /^(\d{4})\/(\d{2})\/(\d{2})$/;
        function dateOk(v) {
            var m = DATE_RE.exec(v);
            if (!m) return false;
            var mo = +m[2], d = +m[3];
            return mo >= 1 && mo <= 12 && d >= 1 && d <= (mo <= 6 ? 31 : 30);
        }
        function normDate(v) {
            v = toLatinDigits(v).trim().replace(/[-.\s]/g, '/');
            if (/^\d{8}$/.test(v)) v = v.slice(0, 4) + '/' + v.slice(4, 6) + '/' + v.slice(6);
            var m = /^(\d{4})\/(\d{1,2})\/(\d{1,2})$/.exec(v);
            if (m) v = m[1] + '/' + ('0' + m[2]).slice(-2) + '/' + ('0' + m[3]).slice(-2);
            return v;
        }

        var coverError = '';
        var rules = {
            Title: function () {
                var v = f('Title').value.trim();
                if (!v) return 'عنوان خبر را وارد کنید.';
                if (v.length < 3) return 'عنوان باید دست‌کم ۳ نویسه باشد.';
                return '';
            },
            Category: function () {
                var v = f('Category').value;
                return (!v || v === '__new__') ? 'یک دسته‌بندی انتخاب کنید.' : '';
            },
            Summary: function () { return f('Summary').value.trim() ? '' : 'خلاصه خبر را وارد کنید.'; },
            Content: function () {
                var len = rteArea.textContent.replace(/\u200b/g, '').trim().length;
                if (!len) return 'متن خبر را بنویسید.';
                if (len < 10) return 'متن خبر خیلی کوتاه است.';
                return '';
            },
            Author: function () { return f('Author').value.trim() ? '' : 'نام نویسنده را وارد کنید.'; },
            ReleaseDate: function () {
                var v = normDate(f('ReleaseDate').value);
                if (!v) return 'تاریخ انتشار را وارد کنید.';
                return dateOk(v) ? '' : 'تاریخ را به‌صورت سال/ماه/روز وارد کنید. مثال: 1405/08/01';
            },
            deadline: function () {
                var v = normDate(f('deadline').value);
                if (!v) return '';
                if (!dateOk(v)) return 'تاریخ را به‌صورت سال/ماه/روز وارد کنید. مثال: 1405/09/01';
                var rd = normDate(f('ReleaseDate').value);
                if (dateOk(rd) && v < rd) return 'مهلت نمی‌تواند قبل از تاریخ انتشار باشد.';
                return '';
            },
            PictureUrl: function () { return coverError; }
        };
        var ORDER = ['Title', 'Category', 'Summary', 'Content', 'PictureUrl', 'Author', 'ReleaseDate', 'deadline'];

        function validateField(name, reveal) {
            if (reveal) shown[name] = true;
            var msg = rules[name]();
            setError(name, shown[name] ? msg : '');
            return msg;
        }
        function validateAll() {
            var bad = [];
            ORDER.forEach(function (n) { if (validateField(n, true)) bad.push(n); });
            return bad;
        }

        /* ----- character counter ----- */
        var counter = $('#summaryCounter');
        function updateCounter() {
            var n = f('Summary').value.length;
            counter.textContent = faNum(n) + ' / ' + faNum(MAX_SUMMARY);
            counter.classList.toggle('is-near', n >= MAX_SUMMARY * 0.9);
        }

        /* ----- category (+ inline "new category") ----- */
        var catSel = f('Category');
        var catAdd = $('#catAdd');
        var catInput = $('#catNewInput');
        var prevCat = '';

        function addCategory(name, select) {
            name = name.trim();
            if (!name) return false;
            var existing = Array.prototype.filter.call(catSel.options, function (o) { return o.value.toLowerCase() === name.toLowerCase(); })[0];
            if (!existing) {
                existing = document.createElement('option');
                existing.value = name;
                existing.textContent = name;
                existing.setAttribute('data-custom', '1');
                catSel.insertBefore(existing, catSel.querySelector('option[value="__new__"]'));
            }
            if (select) catSel.value = existing.value;
            return true;
        }
        catSel.addEventListener('change', function () {
            if (catSel.value === '__new__') {
                catAdd.hidden = false;
                catInput.value = '';
                catInput.focus();
            } else {
                prevCat = catSel.value;
                catAdd.hidden = true;
            }
            if (shown.Category) validateField('Category');
            changed();
        });
        function confirmCat() {
            var v = catInput.value.trim();
            if (v.length < 2) { catInput.classList.add('is-invalid'); catInput.focus(); return; }
            catInput.classList.remove('is-invalid');
            addCategory(v, true);
            prevCat = catSel.value;
            catAdd.hidden = true;
            catSel.focus();
            if (shown.Category) validateField('Category');
            changed();
        }
        function cancelCat() {
            catSel.value = prevCat;
            catAdd.hidden = true;
            catInput.classList.remove('is-invalid');
            catSel.focus();
        }
        $('#catAddOk').addEventListener('click', confirmCat);
        $('#catAddCancel').addEventListener('click', cancelCat);
        catInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); confirmCat(); }
            else if (e.key === 'Escape') { e.preventDefault(); e.stopPropagation(); cancelCat(); }
        });

        /* ----- dynamic lists (DocumentsJson / ConditionsJson) ----- */
        function createList(box) {
            var rowsEl = $('.admin-dyn-rows', box);
            var emptyEl = $('.admin-dyn-empty', box);
            var hidden = $('input[type="hidden"]', box);
            var label = box.getAttribute('data-item-label') || 'مورد';
            var addBtn = $('[data-add]', box);
            var MAX = 20;

            function values() {
                return $$('input', rowsEl).map(function (i) { return i.value.trim(); }).filter(Boolean);
            }
            function relabel() {
                $$('.admin-dyn-row', rowsEl).forEach(function (row, idx) {
                    $('input', row).setAttribute('aria-label', label + ' ' + faNum(idx + 1));
                    $('button', row).setAttribute('aria-label', 'حذف ' + label + ' ' + faNum(idx + 1));
                });
            }
            function sync(silent) {
                hidden.value = JSON.stringify(values());
                emptyEl.hidden = rowsEl.children.length > 0;
                addBtn.disabled = rowsEl.children.length >= MAX;
                if (!silent) changed();
            }
            function addRow(value, focus) {
                if (rowsEl.children.length >= MAX) return;
                var row = document.createElement('div');
                row.className = 'admin-dyn-row';
                row.setAttribute('role', 'listitem');
                var input = document.createElement('input');
                input.type = 'text';
                input.className = 'profile-input';
                input.maxLength = 120;
                input.autocomplete = 'off';
                input.value = value || '';
                var del = document.createElement('button');
                del.type = 'button';
                del.className = 'admin-row-btn admin-row-btn--danger';
                del.appendChild(icon('ad-trash'));
                row.appendChild(input);
                row.appendChild(del);
                rowsEl.appendChild(row);
                input.addEventListener('input', function () { sync(); });
                input.addEventListener('keydown', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        if (input.value.trim()) addRow('', true);
                    }
                });
                del.addEventListener('click', function () {
                    var next = row.nextElementSibling || row.previousElementSibling;
                    rowsEl.removeChild(row);
                    relabel();
                    sync();
                    var target = next ? $('input', next) : addBtn;
                    if (target) target.focus();
                });
                relabel();
                sync(true);
                if (focus) input.focus();
            }
            addBtn.addEventListener('click', function () { addRow('', true); });
            addRow('');
            return {
                get: values,
                set: function (arr) {
                    rowsEl.textContent = '';
                    (arr && arr.length ? arr : ['']).forEach(function (v) { addRow(v); });
                    sync(true);
                }
            };
        }
        var docs = createList($('[data-list="DocumentsJson"]', form));
        var conds = createList($('[data-list="ConditionsJson"]', form));

        /* ----- cover image ----- */
        var coverInput = f('PictureUrl');
        var coverDrop = $('#coverDrop');
        var coverBox = $('#coverBox');
        var coverImg = $('#coverImg');
        var coverName = $('#coverName');
        var pvImg = $('#pvImg');
        var pvIcon = $('#pvIcon');
        var coverUrl = '';

        function releaseCover() { if (coverUrl) { URL.revokeObjectURL(coverUrl); coverUrl = ''; } }
        function clearCover() {
            coverInput.value = '';
            releaseCover();
            coverImg.removeAttribute('src');
            pvImg.removeAttribute('src');
            pvImg.hidden = true;
            pvIcon.hidden = false;
            coverBox.hidden = true;
            coverDrop.hidden = false;
        }
        function handleFile(file) {
            if (!file) return;
            coverError = '';
            if (COVER_TYPES.indexOf(file.type) === -1) coverError = 'فقط فایل‌های jpg، png یا webp پذیرفته می‌شود.';
            else if (file.size > MAX_COVER) coverError = 'حجم تصویر باید حداکثر ۲ مگابایت باشد.';
            if (coverError) {
                clearCover();
                validateField('PictureUrl', true);
                changed();
                return;
            }
            releaseCover();
            coverUrl = URL.createObjectURL(file);
            coverImg.src = coverUrl;
            coverName.textContent = file.name;
            pvImg.src = coverUrl;
            pvImg.hidden = false;
            pvIcon.hidden = true;
            coverBox.hidden = false;
            coverDrop.hidden = true;
            validateField('PictureUrl', true);
            changed();
        }
        coverInput.addEventListener('change', function () { handleFile(coverInput.files && coverInput.files[0]); });
        ['dragenter', 'dragover'].forEach(function (ev) {
            coverDrop.addEventListener(ev, function (e) { e.preventDefault(); coverDrop.classList.add('is-drag'); });
        });
        ['dragleave', 'drop'].forEach(function (ev) {
            coverDrop.addEventListener(ev, function (e) { e.preventDefault(); coverDrop.classList.remove('is-drag'); });
        });
        coverDrop.addEventListener('drop', function (e) {
            var file = e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files[0];
            if (!file) return;
            try { coverInput.files = e.dataTransfer.files; } catch (err) { }
            handleFile(file);
        });
        $('#coverRemove').addEventListener('click', function () {
            coverError = '';
            clearCover();
            setError('PictureUrl', '');
            coverInput.focus();
            changed();
        });
        window.addEventListener('pagehide', releaseCover);

        /* ----- rich-text editor (contentEditable) ----- */
        try { document.execCommand('defaultParagraphSeparator', false, 'p'); } catch (e) { }

        var BLOCK_OUT = { P: 1, H2: 1, H3: 1, UL: 1, OL: 1, BLOCKQUOTE: 1 };
        var HEAD_MAP = { H1: 'h2', H2: 'h2', H3: 'h3', H4: 'h3', H5: 'h3', H6: 'h3' };
        var INLINE_MAP = { STRONG: 'strong', B: 'strong', EM: 'em', I: 'em', U: 'u' };
        var DROP = { SCRIPT: 1, STYLE: 1, NOSCRIPT: 1, IFRAME: 1, OBJECT: 1, EMBED: 1, TEMPLATE: 1, SVG: 1, CANVAS: 1, AUDIO: 1, VIDEO: 1, IMG: 1, INPUT: 1, BUTTON: 1, SELECT: 1, TEXTAREA: 1, LINK: 1, META: 1, HEAD: 1, TITLE: 1 };
        var BLOCKISH = { DIV: 1, SECTION: 1, ARTICLE: 1, HEADER: 1, FOOTER: 1, MAIN: 1, ASIDE: 1, FIGURE: 1, PRE: 1, TABLE: 1, TR: 1, DL: 1, DT: 1, DD: 1, FORM: 1, NAV: 1, FIGCAPTION: 1 };

        function safeHref(h) {
            h = (h || '').trim();
            return /^(https?:\/\/|mailto:|tel:|\/|#)/i.test(h) ? h : '';
        }

        /* allow-list sanitizer: returns clean HTML (p, h2, h3, strong, em, u, ul, ol, li, blockquote, a, br) */
        function sanitize(html) {
            var doc = new DOMParser().parseFromString('<body>' + html + '</body>', 'text/html');
            var out = document.createElement('div');

            function build(node, parent) {
                if (node.nodeType === 3) { parent.appendChild(document.createTextNode(node.nodeValue)); return; }
                if (node.nodeType !== 1) return;
                var tag = node.tagName;
                var ptag = parent.tagName ? parent.tagName.toLowerCase() : 'div';
                if (DROP[tag]) return;
                if (tag === 'BR') { if (parent !== out) parent.appendChild(document.createElement('br')); return; }
                var el;
                if (HEAD_MAP[tag]) {
                    if (parent === out) { el = document.createElement(HEAD_MAP[tag]); kids(node, el); out.appendChild(el); }
                    else kids(node, parent);
                    return;
                }
                if (tag === 'P' || tag === 'BLOCKQUOTE') {
                    if (parent === out || (tag === 'BLOCKQUOTE' && (ptag === 'blockquote' || ptag === 'li'))) {
                        el = document.createElement(tag.toLowerCase()); kids(node, el); parent.appendChild(el);
                    } else kids(node, parent);
                    return;
                }
                if (tag === 'UL' || tag === 'OL') {
                    if (parent === out || ptag === 'li') {
                        el = document.createElement(tag.toLowerCase());
                        Array.prototype.forEach.call(node.childNodes, function (c) { if (c.nodeType === 1) build(c, el); });
                        if (el.children.length) parent.appendChild(el);
                    } else kids(node, parent);
                    return;
                }
                if (tag === 'LI') {
                    if (ptag === 'ul' || ptag === 'ol') { el = document.createElement('li'); kids(node, el); parent.appendChild(el); }
                    else if (parent === out) { el = document.createElement('p'); kids(node, el); out.appendChild(el); }
                    else kids(node, parent);
                    return;
                }
                if (INLINE_MAP[tag]) { el = document.createElement(INLINE_MAP[tag]); kids(node, el); parent.appendChild(el); return; }
                if (tag === 'A') {
                    var href = safeHref(node.getAttribute('href'));
                    if (href) {
                        el = document.createElement('a');
                        el.setAttribute('href', href);
                        el.setAttribute('target', '_blank');
                        el.setAttribute('rel', 'noopener noreferrer');
                        kids(node, el);
                        parent.appendChild(el);
                    } else kids(node, parent);
                    return;
                }
                if (BLOCKISH[tag] && parent === out) { el = document.createElement('p'); kids(node, el); out.appendChild(el); return; }
                kids(node, parent); // span, font, nested div...: unwrap
            }
            function kids(node, parent) { Array.prototype.forEach.call(node.childNodes, function (c) { build(c, parent); }); }
            kids(doc.body, out);

            // wrap loose top-level inline content into paragraphs; drop empty blocks
            var res = document.createElement('div');
            var cur = null;
            Array.prototype.forEach.call(out.childNodes, function (n) {
                var isBlock = n.nodeType === 1 && BLOCK_OUT[n.tagName];
                if (isBlock) { cur = null; if (n.textContent.trim() || n.querySelector('li')) res.appendChild(n); return; }
                if (n.nodeType === 3 && !n.nodeValue.trim() && !cur) return;
                if (!cur) { cur = document.createElement('p'); res.appendChild(cur); }
                cur.appendChild(n);
            });
            Array.prototype.slice.call(res.querySelectorAll('p')).forEach(function (p) { if (!p.textContent.trim()) p.parentNode.removeChild(p); });
            return res.innerHTML;
        }

        function escapeHtml(s) { return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'); }
        function textToHtml(t) {
            return t.replace(/\r/g, '').split(/\n{2,}/).map(function (p) {
                p = p.trim();
                return p ? '<p>' + escapeHtml(p).replace(/\n/g, '<br>') + '</p>' : '';
            }).join('');
        }

        function rteIsEmpty() {
            return !rteArea.textContent.replace(/\u200b/g, '').trim() && !rteArea.querySelector('li, hr');
        }
        function syncContent() {
            rteArea.classList.toggle('is-empty', rteIsEmpty());
            contentInput.value = rteIsEmpty() ? '' : sanitize(rteArea.innerHTML);
        }

        rteArea.addEventListener('input', function () {
            syncContent();
            if (shown.Content) validateField('Content');
            changed();
        });
        rteArea.addEventListener('blur', function () { validateField('Content', true); });
        rteArea.addEventListener('paste', function (e) {
            e.preventDefault();
            var cd = e.clipboardData || window.clipboardData;
            if (!cd) return;
            var html = cd.getData('text/html');
            var text = cd.getData('text/plain');
            var ins = html ? sanitize(html) : '';
            if (!ins && text) ins = textToHtml(text);
            if (ins) document.execCommand('insertHTML', false, ins);
        });
        rteArea.addEventListener('drop', function (e) {
            e.preventDefault();
            var text = e.dataTransfer && e.dataTransfer.getData('text/plain');
            if (!text) return;
            var range = null;
            if (document.caretRangeFromPoint) range = document.caretRangeFromPoint(e.clientX, e.clientY);
            else if (document.caretPositionFromPoint) {
                var pos = document.caretPositionFromPoint(e.clientX, e.clientY);
                if (pos) { range = document.createRange(); range.setStart(pos.offsetNode, pos.offset); range.collapse(true); }
            }
            rteArea.focus();
            if (range) { var s = window.getSelection(); s.removeAllRanges(); s.addRange(range); }
            document.execCommand('insertText', false, text);
        });

        /* selection helpers */
        var savedRange = null;
        function selInRte() {
            var s = window.getSelection();
            return s && s.rangeCount && rteArea.contains(s.anchorNode);
        }
        function closestTag(tags) {
            var s = window.getSelection();
            var n = s && s.anchorNode;
            while (n && n !== rteArea) {
                if (n.nodeType === 1 && tags.indexOf(n.tagName.toLowerCase()) !== -1) return n;
                n = n.parentNode;
            }
            return null;
        }
        function restoreRange() {
            rteArea.focus();
            if (savedRange) { var s = window.getSelection(); s.removeAllRanges(); s.addRange(savedRange); }
        }

        /* toolbar */
        var bar = $('.admin-rte-bar', form);
        var barBtns = $$('.admin-rte-btn', bar);
        var activeBtn = 0;
        function setRoving(i) {
            activeBtn = i;
            barBtns.forEach(function (b, idx) { b.tabIndex = idx === i ? 0 : -1; });
        }
        setRoving(0);
        bar.addEventListener('mousedown', function (e) { if (e.target.closest('.admin-rte-btn')) e.preventDefault(); });
        bar.addEventListener('keydown', function (e) {
            var i = activeBtn;
            if (e.key === 'ArrowLeft') i = (i + 1) % barBtns.length;           // RTL: left = next
            else if (e.key === 'ArrowRight') i = (i - 1 + barBtns.length) % barBtns.length;
            else if (e.key === 'Home') i = 0;
            else if (e.key === 'End') i = barBtns.length - 1;
            else return;
            e.preventDefault();
            setRoving(i);
            barBtns[i].focus();
        });

        function pressed(cmd, on) {
            var b = $('[data-cmd="' + cmd + '"]', bar);
            if (b) b.setAttribute('aria-pressed', on ? 'true' : 'false');
        }
        ['h2', 'h3', 'bold', 'italic', 'underline', 'ul', 'ol', 'quote', 'link'].forEach(function (c) { pressed(c, false); });

        function updateToolbar() {
            if (!selInRte()) return;
            try {
                pressed('bold', document.queryCommandState('bold'));
                pressed('italic', document.queryCommandState('italic'));
                pressed('underline', document.queryCommandState('underline'));
                pressed('ul', document.queryCommandState('insertUnorderedList'));
                pressed('ol', document.queryCommandState('insertOrderedList'));
            } catch (e) { }
            pressed('h2', !!closestTag(['h2']));
            pressed('h3', !!closestTag(['h3']));
            pressed('quote', !!closestTag(['blockquote']));
            pressed('link', !!closestTag(['a']));
        }
        document.addEventListener('selectionchange', function () {
            if (selInRte()) {
                savedRange = window.getSelection().getRangeAt(0).cloneRange();
                updateToolbar();
            }
        });

        function toggleBlock(tag) {
            if (tag === 'blockquote') {
                if (closestTag(['blockquote'])) document.execCommand('outdent');
                else document.execCommand('formatBlock', false, '<blockquote>');
                return;
            }
            document.execCommand('formatBlock', false, closestTag([tag]) ? '<p>' : '<' + tag + '>');
        }

        var linkBar = $('#rteLinkBar');
        var linkInput = $('#rteLinkInput');
        function openLinkBar() {
            var a = closestTag(['a']);
            linkInput.value = a ? (a.getAttribute('href') || '') : '';
            linkInput.classList.remove('is-invalid');
            linkBar.hidden = false;
            linkInput.focus();
            linkInput.select();
        }
        function closeLinkBar(refocus) {
            linkBar.hidden = true;
            if (refocus) restoreRange();
        }
        function applyLink() {
            var v = linkInput.value.trim();
            if (!v) { linkInput.classList.add('is-invalid'); linkInput.focus(); return; }
            if (!/^(https?:\/\/|mailto:|tel:|\/|#)/i.test(v)) v = 'https://' + v;
            var ok = false;
            try { var u = new URL(v, window.location.origin); ok = /^(https?:|mailto:|tel:)$/.test(u.protocol); } catch (e) { }
            if (!ok) { linkInput.classList.add('is-invalid'); linkInput.focus(); return; }
            restoreRange();
            var collapsed = savedRange && savedRange.collapsed;
            if (collapsed && !closestTag(['a'])) document.execCommand('insertHTML', false, '<a href="' + escapeHtml(v).replace(/"/g, '&quot;') + '">' + escapeHtml(v) + '</a>');
            else document.execCommand('createLink', false, v);
            linkBar.hidden = true;
            syncContent();
            changed();
        }
        $('#rteLinkOk').addEventListener('click', applyLink);
        $('#rteLinkRemove').addEventListener('click', function () {
            restoreRange();
            var a = closestTag(['a']);
            if (a) {
                var r = document.createRange();
                r.selectNodeContents(a);
                var s = window.getSelection();
                s.removeAllRanges(); s.addRange(r);
            }
            document.execCommand('unlink');
            linkBar.hidden = true;
            syncContent();
            changed();
        });
        $('#rteLinkCancel').addEventListener('click', function () { closeLinkBar(true); });
        linkInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); applyLink(); }
            else if (e.key === 'Escape') { e.preventDefault(); e.stopPropagation(); closeLinkBar(true); }
        });

        bar.addEventListener('click', function (e) {
            var b = e.target.closest('.admin-rte-btn');
            if (!b) return;
            setRoving(barBtns.indexOf(b));
            var cmd = b.getAttribute('data-cmd');
            if (cmd === 'link') { openLinkBar(); return; }
            restoreRange();
            switch (cmd) {
                case 'h2': case 'h3': toggleBlock(cmd); break;
                case 'quote': toggleBlock('blockquote'); break;
                case 'bold': document.execCommand('bold'); break;
                case 'italic': document.execCommand('italic'); break;
                case 'underline': document.execCommand('underline'); break;
                case 'ul': document.execCommand('insertUnorderedList'); break;
                case 'ol': document.execCommand('insertOrderedList'); break;
                case 'undo': document.execCommand('undo'); break;
                case 'redo': document.execCommand('redo'); break;
                case 'clear': document.execCommand('removeFormat'); document.execCommand('formatBlock', false, '<p>'); break;
            }
            syncContent();
            updateToolbar();
            changed();
        });

        /* ----- live preview (same classes as _NewsCard) ----- */
        var pv = {
            title: $('#pvTitle'), summary: $('#pvSummary'), date: $('#pvDate'), isNew: $('#pvNew')
        };
        function updatePreview() {
            pv.title.textContent = f('Title').value.trim() || 'عنوان خبر اینجا نمایش داده می‌شود';
            pv.summary.textContent = f('Summary').value.trim() || 'خلاصه خبر اینجا نمایش داده می‌شود.';
            pv.date.textContent = normDate(f('ReleaseDate').value) || '—';
            pv.isNew.hidden = !f('IsNew').checked;
        }

        /* ----- state, draft autosave, unsaved-changes warning ----- */
        function getState() {
            var customCats = Array.prototype.filter.call(catSel.options, function (o) { return o.hasAttribute('data-custom'); }).map(function (o) { return o.value; });
            return {
                Title: f('Title').value,
                Category: catSel.value === '__new__' ? '' : catSel.value,
                Summary: f('Summary').value,
                Content: contentInput.value,
                Author: f('Author').value,
                ReleaseDate: f('ReleaseDate').value,
                deadline: f('deadline').value,
                IsNew: f('IsNew').checked,
                docs: docs.get(),
                conds: conds.get(),
                cats: customCats
            };
        }
        function snapshot() {
            var s = getState();
            s.cover = coverInput.files && coverInput.files[0] ? coverInput.files[0].name + ':' + coverInput.files[0].size : '';
            return JSON.stringify(s);
        }
        function isBlank(s) {
            return !s.Title.trim() && !s.Summary.trim() && !s.Content && !s.Category && !s.deadline.trim() && !s.docs.length && !s.conds.length;
        }
        var baseline = '';
        function isDirty() { return snapshot() !== baseline; }
        function setStatus(msg) { statusEl.textContent = msg; }

        function saveDraft() {
            var s = getState();
            try {
                if (isBlank(s)) localStorage.removeItem(DRAFT_KEY);
                else localStorage.setItem(DRAFT_KEY, JSON.stringify({ v: 1, savedAt: Date.now(), state: s }));
                return true;
            } catch (e) { return false; }
        }
        var autosaveTimer = null;
        function scheduleAutosave() {
            clearTimeout(autosaveTimer);
            autosaveTimer = setTimeout(function () {
                if (saveDraft() && !isBlank(getState())) setStatus('ذخیره خودکار در مرورگر انجام شد (' + faTime(new Date()) + ')');
            }, 700);
        }
        function changed() {
            updatePreview();
            updateCounter();
            scheduleAutosave();
        }

        function applyState(s) {
            f('Title').value = s.Title || '';
            (s.cats || []).forEach(function (c) { addCategory(c, false); });
            catSel.value = s.Category || '';
            if (catSel.value !== (s.Category || '')) catSel.value = '';
            prevCat = catSel.value;
            f('Summary').value = s.Summary || '';
            rteArea.innerHTML = s.Content ? sanitize(s.Content) : '';
            if (s.Author) f('Author').value = s.Author;
            if (s.ReleaseDate) f('ReleaseDate').value = s.ReleaseDate;
            f('deadline').value = s.deadline || '';
            f('IsNew').checked = !!s.IsNew;
            docs.set(s.docs);
            conds.set(s.conds);
            syncContent();
            updatePreview();
            updateCounter();
        }
        function restoreDraft() {
            try {
                var raw = localStorage.getItem(DRAFT_KEY);
                if (!raw) return false;
                var d = JSON.parse(raw);
                if (!d || !d.state || isBlank(d.state)) return false;
                applyState(d.state);
                banner.hidden = false;
                setStatus('پیش‌نویس ذخیره‌شده بازیابی شد (' + faTime(new Date(d.savedAt)) + ')');
                return true;
            } catch (e) { return false; }
        }
        $('#btnDiscardDraft').addEventListener('click', function () {
            try { localStorage.removeItem(DRAFT_KEY); } catch (e) { }
            form.reset();
            catSel.value = '';
            catAdd.hidden = true;
            f('ReleaseDate').value = todayJalali();
            rteArea.innerHTML = '';
            docs.set([]);
            conds.set([]);
            clearCover();
            coverError = '';
            ORDER.forEach(function (n) { shown[n] = false; setError(n, ''); });
            syncContent();
            updatePreview();
            updateCounter();
            banner.hidden = true;
            baseline = snapshot();
            setStatus('پیش‌نویس حذف شد');
            f('Title').focus();
        });

        $('#btnSaveDraft').addEventListener('click', function () {
            if (saveDraft()) {
                baseline = snapshot();
                setStatus('پیش‌نویس ذخیره شد (' + faTime(new Date()) + ')');
                toast('پیش‌نویس فقط در همین مرورگر ذخیره شد، نه روی سرور.', 'success', 4000);
            } else {
                toast('ذخیره پیش‌نویس در مرورگر ممکن نشد.', 'error');
            }
        });

        window.addEventListener('beforeunload', function (e) {
            if (isDirty()) { e.preventDefault(); e.returnValue = ''; }
        });

        /* ----- full preview modal ----- */
        var pvBody = $('#postPreviewBody');
        function add(parent, tag, cls, text) {
            var el = document.createElement(tag);
            if (cls) el.className = cls;
            if (text != null) el.textContent = text;
            parent.appendChild(el);
            return el;
        }
        function listBlock(title, items) {
            if (!items.length) return;
            add(pvBody, 'h4', null, title);
            var ul = add(pvBody, 'ul');
            items.forEach(function (t) { add(ul, 'li', null, t); });
        }
        $('#btnPreview').addEventListener('click', function () {
            var s = getState();
            pvBody.textContent = '';
            if (s.Category) add(pvBody, 'span', 'admin-tag admin-tag--ok admin-article-cat', s.Category);
            add(pvBody, 'h2', 'admin-article-title', s.Title.trim() || 'عنوان خبر');
            var meta = add(pvBody, 'p', 'admin-article-meta');
            add(meta, 'span', null, 'نویسنده: ' + (s.Author.trim() || '—'));
            add(meta, 'span', null, 'تاریخ انتشار: ' + (normDate(s.ReleaseDate) || '—'));
            if (normDate(s.deadline)) add(meta, 'span', null, 'مهلت: ' + normDate(s.deadline));
            if (coverUrl) { var im = add(pvBody, 'img', 'admin-article-cover'); im.src = coverUrl; im.alt = ''; }
            if (s.Summary.trim()) add(pvBody, 'p', 'admin-article-summary', s.Summary.trim());
            var body = add(pvBody, 'div');
            if (s.Content) body.innerHTML = sanitize(s.Content); // sanitized by the allow-list above
            else add(body, 'p', null, 'هنوز متنی نوشته نشده است.');
            listBlock('مدارک مورد نیاز', s.docs);
            listBlock('شرایط', s.conds);
            openModal(previewModal, $('#btnPreview'));
        });

        /* ----- events: live validation, date normalization ----- */
        ['Title', 'Summary', 'Author', 'ReleaseDate', 'deadline'].forEach(function (n) {
            f(n).addEventListener('input', function () {
                if ((n === 'ReleaseDate' || n === 'deadline') && /[\u06F0-\u06F9\u0660-\u0669]/.test(f(n).value)) f(n).value = toLatinDigits(f(n).value);
                if (shown[n]) validateField(n);
                if (n === 'ReleaseDate' && shown.deadline) validateField('deadline');
                changed();
            });
            f(n).addEventListener('blur', function () {
                if (n === 'ReleaseDate' || n === 'deadline') { f(n).value = normDate(f(n).value); }
                validateField(n, true);
                if (n === 'ReleaseDate' && f('deadline').value) validateField('deadline');
                changed();
            });
        });
        f('IsNew').addEventListener('change', changed);

        form.addEventListener('submit', function (e) {
            e.preventDefault(); // BACKEND-TODO: remove when the real POST exists
            f('ReleaseDate').value = normDate(f('ReleaseDate').value);
            f('deadline').value = normDate(f('deadline').value);
            syncContent();
            var bad = validateAll();
            if (bad.length) {
                toast('فرم خطا دارد. موارد مشخص‌شده را اصلاح کنید.', 'error', 4500);
                var first = bad[0];
                var target = first === 'Content' ? rteArea : (first === 'PictureUrl' ? $('#coverDrop') : f(first));
                if (target) { target.scrollIntoView({ block: 'center', behavior: reduceMotion ? 'auto' : 'smooth' }); if (first === 'PictureUrl') coverInput.focus(); else target.focus({ preventScroll: true }); }
                return;
            }
            toast(UI_PREVIEW_NOTICE, 'success', 8000); // BACKEND-TODO: replace with the real success flow
        });

        /* ----- init ----- */
        syncContent();
        updatePreview();
        updateCounter();
        restoreDraft();
        baseline = snapshot();
    })();
})();
