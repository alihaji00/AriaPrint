/* =========================================================
   SUBMIT A REQUEST — front-end only
   Everything that would normally talk to the server is mocked
   and marked with  "BACKEND HOOK".
   ========================================================= */
(function () {
    'use strict';

    /* ---------- configuration (mock data) ----------
       Each request type decides which fields and which uploads are shown.
       field : { name, label, type: text|number|select, required, placeholder, options, full, pattern, patternMsg }
       file  : { key, label, help, required }                                                       */
    var REQUEST_TYPES = [
        {
            id: 'fuel_card',
            label: 'درخواست کارت سوخت',
            hint: 'برای ثبت درخواست کارت سوخت، تصویر مدرک مالکیت خودرو و صفحه‌ی اول شناسنامه الزامی است.',
            fields: [
                { name: 'nationalCode', label: 'کد ملی', type: 'text', required: true, placeholder: '۱۰ رقم', digits: 10, patternMsg: 'کد ملی باید ۱۰ رقم باشد.' },
                { name: 'plate', label: 'شماره پلاک خودرو', type: 'text', required: false, placeholder: 'مثلاً ۱۲ ب ۳۴۵ ایران ۶۷' }
            ],
            files: [
                { key: 'carOwnership', label: 'مدرک مالکیت خودرو', help: 'سند یا کارت خودرو (واضح و کامل)', required: true },
                { key: 'birthCert', label: 'صفحه‌ی اول شناسنامه', help: 'تصویر خوانا از صفحه‌ی اول', required: true }
            ]
        },
        {
            id: 'print_copy',
            label: 'سفارش پرینت و کپی',
            hint: 'فایل خود را بارگذاری کنید و مشخصات چاپ را وارد کنید.',
            fields: [
                { name: 'pages', label: 'تعداد صفحات', type: 'number', required: true, placeholder: 'مثلاً ۲۵', patternMsg: 'تعداد صفحات را به‌صورت عدد وارد کنید.' },
                { name: 'color', label: 'نوع چاپ', type: 'select', required: true, options: ['سیاه و سفید', 'رنگی'] }
            ],
            files: [
                { key: 'printFile', label: 'فایل مورد نظر برای چاپ', help: 'PDF یا تصویر', required: true }
            ]
        },
        {
            id: 'scan_type',
            label: 'اسکن و تایپ',
            hint: 'سند خود را بارگذاری کنید تا برای اسکن یا تایپ آماده شود.',
            fields: [
                { name: 'service', label: 'نوع خدمت', type: 'select', required: true, options: ['فقط اسکن', 'تایپ فارسی', 'تایپ انگلیسی'] },
                { name: 'deadline', label: 'زمان مورد نیاز', type: 'select', required: false, options: ['عادی', 'فوری (همان روز)'] }
            ],
            files: [
                { key: 'sourceDoc', label: 'سند اصلی', help: 'تصویر یا PDF خوانا', required: true }
            ]
        },
        {
            id: 'student',
            label: 'خدمات دانشجویی',
            hint: 'برای خدمات دانشجویی، اطلاعات تحصیلی خود را کامل کنید.',
            fields: [
                { name: 'university', label: 'نام دانشگاه', type: 'text', required: true, placeholder: 'مثلاً دانشگاه آزاد بهبهان' },
                { name: 'major', label: 'رشته تحصیلی', type: 'text', required: true, placeholder: 'مثلاً مهندسی کامپیوتر' },
                { name: 'topic', label: 'موضوع درخواست', type: 'text', required: true, full: true, placeholder: 'مثلاً تایپ و صفحه‌آرایی پایان‌نامه' }
            ],
            files: [
                { key: 'studentCard', label: 'کارت دانشجویی', help: 'اختیاری، ولی سرعت رسیدگی را بیشتر می‌کند', required: false }
            ]
        },
        {
            id: 'bills',
            label: 'پرداخت قبوض',
            hint: 'این نوع درخواست نیازی به بارگذاری مدرک ندارد.',
            fields: [
                { name: 'billType', label: 'نوع قبض', type: 'select', required: true, options: ['برق', 'گاز', 'آب', 'تلفن', 'سایر'] },
                { name: 'billId', label: 'شناسه قبض', type: 'text', required: true, placeholder: 'شناسه قبض را وارد کنید' },
                { name: 'payId', label: 'شناسه پرداخت', type: 'text', required: false, placeholder: 'در صورت وجود' }
            ],
            files: []
        }
    ];

    var MAX_SIZE = 5 * 1024 * 1024; // 5 MB
    var ALLOWED_EXT = ['jpg', 'jpeg', 'png', 'webp', 'pdf'];
    var NOTES_MAX = 1500;

    /* ---------- helpers ---------- */
    function $(sel, root) { return (root || document).querySelector(sel); }
    function $$(sel, root) { return Array.prototype.slice.call((root || document).querySelectorAll(sel)); }

    var FA = '۰۱۲۳۴۵۶۷۸۹';
    function toFa(n) { return String(n).replace(/\d/g, function (d) { return FA[d]; }); }
    function toEn(s) {
        return String(s)
            .replace(/[۰-۹]/g, function (d) { return FA.indexOf(d); })
            .replace(/[٠-٩]/g, function (d) { return '٠١٢٣٤٥٦٧٨٩'.indexOf(d); });
    }
    function formatSize(bytes) {
        if (bytes < 1024 * 1024) return toFa(Math.max(1, Math.round(bytes / 1024))) + ' کیلوبایت';
        return toFa((bytes / 1024 / 1024).toFixed(1)).replace('.', '٫') + ' مگابایت';
    }
    function extOf(name) { var m = /\.([^.]+)$/.exec(name || ''); return m ? m[1].toLowerCase() : ''; }
    function isImage(file) { return /^image\//.test(file.type) || ['jpg', 'jpeg', 'png', 'webp'].indexOf(extOf(file.name)) > -1; }

    var ICONS = {
        upload: '<path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/>',
        file: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/>',
        image: '<rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/>',
        check: '<polyline points="20 6 9 17 4 12"/>',
        x: '<line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>',
        expand: '<polyline points="15 3 21 3 21 9"/><polyline points="9 21 3 21 3 15"/><line x1="21" y1="3" x2="14" y2="10"/><line x1="3" y1="21" x2="10" y2="14"/>',
        trash: '<polyline points="3 6 5 6 21 6"/><path d="M19 6l-2 14a2 2 0 0 1-2 2H9a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/>',
        refresh: '<polyline points="23 4 23 10 17 10"/><path d="M20.5 15a9 9 0 1 1-2.1-9.4L23 10"/>',
        info: '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>',
        alert: '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="13"/><line x1="12" y1="16.5" x2="12.01" y2="16.5"/>'
    };
    function icon(name, cls) {
        return '<svg class="req-ico ' + (cls || '') + '" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' + ICONS[name] + '</svg>';
    }
    function el(tag, cls, text) {
        var n = document.createElement(tag);
        if (cls) n.className = cls;
        if (text != null) n.textContent = text;
        return n;
    }

    /* ---------- elements ---------- */
    var form = $('#requestForm');
    if (!form) return;

    var typeSelect = $('#requestType');
    var typeHint = $('#typeHint');
    var afterType = $('#afterType');
    var detailsCard = $('#detailsCard');
    var dynamicFields = $('#dynamicFields');
    var dynamicFiles = $('#dynamicFiles');
    var phoneRadios = $$('input[name="phoneMode"]');
    var newPhoneWrap = $('#newPhoneWrap');
    var newPhone = $('#newPhone');
    var notes = $('#notes');
    var notesCounter = $('#notesCounter');
    var formAlert = $('#formAlert');
    var submitBtn = $('#submitBtn');
    var success = $('#reqSuccess');
    var live = $('#reqLive');

    var state = { type: null, slots: {} };

    /* ---------- request type ---------- */
    REQUEST_TYPES.forEach(function (t) {
        var o = el('option', null, t.label);
        o.value = t.id;
        typeSelect.appendChild(o);
    });

    typeSelect.addEventListener('change', function () {
        var t = REQUEST_TYPES.filter(function (x) { return x.id === typeSelect.value; })[0];
        if (!t) return;
        clearError(typeSelect.closest('.profile-field'));
        selectType(t);
    });

    function selectType(t) {
        resetDynamic();
        state.type = t;

        typeHint.innerHTML = icon('info') + '<span></span>';
        $('span', typeHint).textContent = t.hint;
        typeHint.hidden = false;

        t.fields.forEach(function (f) { dynamicFields.appendChild(buildField(f)); });
        if (t.files.length) {
            dynamicFiles.appendChild(el('p', 'req-files-title', 'مدارک مورد نیاز'));
            t.files.forEach(function (def) { dynamicFiles.appendChild(buildSlot(def)); });
        }

        dynamicFields.hidden = !t.fields.length;
        dynamicFiles.hidden = !t.files.length;

        if (afterType.hidden) {
            afterType.hidden = false;
            afterType.classList.add('req-reveal');
        }
        detailsCard.classList.remove('req-reveal');
        void detailsCard.offsetWidth;            // restart the animation
        detailsCard.classList.add('req-reveal');

        hideAlert();
        announce('فیلدها و مدارک مربوط به «' + t.label + '» نمایش داده شد.');
    }

    function resetDynamic() {
        Object.keys(state.slots).forEach(function (k) { destroySlot(state.slots[k]); });
        state.slots = {};
        dynamicFields.innerHTML = '';
        dynamicFiles.innerHTML = '';
    }

    /* ---------- dynamic fields ---------- */
    function buildField(f) {
        var wrap = el('div', 'profile-field' + (f.full ? ' profile-field--full' : ''));
        wrap.dataset.field = f.name;
        var id = 'f-' + f.name;

        var label = el('label', null, f.label);
        label.htmlFor = id;
        if (f.required) label.appendChild(el('em', 'req-required', '*'));
        else label.appendChild(el('span', 'req-optional', ' (اختیاری)'));
        wrap.appendChild(label);

        var input;
        if (f.type === 'select') {
            var sw = el('div', 'req-select-wrap');
            input = el('select', 'profile-input req-select');
            var ph = el('option', null, 'انتخاب کنید…');
            ph.value = ''; ph.disabled = true; ph.selected = true;
            input.appendChild(ph);
            f.options.forEach(function (opt) { var o = el('option', null, opt); o.value = opt; input.appendChild(o); });
            input.required = true;                      // lets :invalid style the placeholder
            if (!f.required) { input.required = false; ph.disabled = false; ph.textContent = '—'; }
            sw.appendChild(input);
            wrap.appendChild(sw);
        } else {
            input = el('input', 'profile-input');
            input.type = 'text';
            input.inputMode = (f.type === 'number' || f.digits) ? 'numeric' : 'text';
            if (f.placeholder) input.placeholder = f.placeholder;
            if (f.digits) input.maxLength = f.digits;
            input.autocomplete = 'off';
            if (f.type === 'number' || f.digits) {
                input.addEventListener('input', function () {
                    input.value = toEn(input.value).replace(/\D/g, '');
                });
            }
            wrap.appendChild(input);
        }
        input.id = id;
        input.name = f.name;
        input.addEventListener('input', function () { clearError(wrap); });
        input.addEventListener('change', function () { clearError(wrap); });

        wrap.appendChild(errorNode());
        wrap._def = f;
        wrap._input = input;
        return wrap;
    }

    /* ---------- file slots ---------- */
    function buildSlot(def) {
        var root = el('div', 'req-file');
        root.dataset.state = 'empty';
        root.dataset.key = def.key;
        var inputId = 'file-' + def.key;

        root.innerHTML =
            '<div class="req-file-head"><span class="req-file-label"></span><span class="req-file-help"></span></div>' +
            '<input type="file" class="req-file-input" id="' + inputId + '" accept=".jpg,.jpeg,.png,.webp,.pdf,image/*,application/pdf" hidden>' +

            '<div class="req-pane req-pane--empty">' +
              '<button type="button" class="req-drop" data-act="pick">' + icon('upload') + '<span>انتخاب فایل یا رها کردن آن در این‌جا</span></button>' +
            '</div>' +

            '<div class="req-pane req-pane--staged">' +
              '<div class="req-fileinfo">' + icon('file', 'req-ico--lg') +
                '<div class="req-fmeta"><span class="req-fname" data-role="stName"></span><span class="req-fsize" data-role="stSize"></span></div></div>' +
              '<div class="req-actions">' +
                '<button type="button" class="req-mini-btn req-mini-btn--primary" data-act="upload">' + icon('upload', 'req-ico--sm') + '<span>آپلود</span></button>' +
                '<button type="button" class="req-mini-btn" data-act="cancel">' + icon('x', 'req-ico--sm') + '<span>لغو</span></button>' +
              '</div>' +
            '</div>' +

            '<div class="req-pane req-pane--uploading">' +
              '<span class="req-fname" data-role="upName"></span>' +
              '<div class="req-progress" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow="0"><span></span></div>' +
              '<span class="req-fsize">در حال آپلود…</span>' +
            '</div>' +

            '<div class="req-pane req-pane--uploaded">' +
              '<button type="button" class="req-thumb" data-act="preview" aria-label="مشاهده‌ی تمام‌صفحه‌ی فایل"></button>' +
              '<div class="req-fileinfo"><div class="req-fmeta">' +
                '<span class="req-fname" data-role="doName"></span><span class="req-fsize" data-role="doSize"></span>' +
                '<span class="req-badge-ok">' + icon('check', 'req-ico--sm') + 'آپلود شد</span></div></div>' +
              '<div class="req-actions">' +
                '<button type="button" class="req-mini-btn" data-act="pick">' + icon('refresh', 'req-ico--sm') + '<span>تعویض</span></button>' +
                '<button type="button" class="req-mini-btn req-mini-btn--danger" data-act="remove">' + icon('trash', 'req-ico--sm') + '<span>حذف</span></button>' +
              '</div>' +
            '</div>' +

            '<p class="req-error" hidden></p>';

        $('.req-file-label', root).textContent = def.label;
        if (def.required) $('.req-file-label', root).appendChild(el('em', 'req-required', '*'));
        else $('.req-file-label', root).appendChild(el('span', 'req-optional', ' (اختیاری)'));
        $('.req-file-help', root).textContent = def.help + ' · فرمت مجاز: تصویر یا PDF، حداکثر ' + toFa(5) + ' مگابایت';

        var slot = { def: def, root: root, input: $('.req-file-input', root), staged: null, uploaded: null, timer: null };
        state.slots[def.key] = slot;

        slot.input.addEventListener('change', function () {
            var f = slot.input.files && slot.input.files[0];
            if (f) stage(slot, f);
            slot.input.value = '';          // allows re-selecting the same file
        });

        root.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-act]');
            if (!btn || !root.contains(btn)) return;
            var act = btn.dataset.act;
            if (act === 'pick') slot.input.click();
            else if (act === 'upload') startUpload(slot);
            else if (act === 'cancel') cancelStage(slot);
            else if (act === 'remove') removeFile(slot);
            else if (act === 'preview') openViewer(slot, btn);
        });

        // drag & drop
        ['dragenter', 'dragover'].forEach(function (ev) {
            root.addEventListener(ev, function (e) {
                if (slot.root.dataset.state === 'uploading') return;
                e.preventDefault();
                root.classList.add('is-dragover');
            });
        });
        ['dragleave', 'drop'].forEach(function (ev) {
            root.addEventListener(ev, function (e) {
                e.preventDefault();
                root.classList.remove('is-dragover');
            });
        });
        root.addEventListener('drop', function (e) {
            if (slot.root.dataset.state === 'uploading') return;
            var f = e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files[0];
            if (f) stage(slot, f);
        });

        return root;
    }

    function setState(slot, s) { slot.root.dataset.state = s; }

    function slotError(slot, msg) {
        var p = $('.req-error', slot.root);
        if (!msg) { p.hidden = true; p.innerHTML = ''; slot.root.classList.remove('is-invalid'); return; }
        p.innerHTML = icon('alert', 'req-ico--sm') + '<span></span>';
        $('span', p).textContent = msg;
        p.hidden = false;
        slot.root.classList.add('is-invalid');
    }

    function stage(slot, file) {
        if (ALLOWED_EXT.indexOf(extOf(file.name)) === -1) {
            slotError(slot, 'فرمت فایل مجاز نیست. فقط فایل تصویری یا PDF قابل قبول است.');
            return;
        }
        if (file.size > MAX_SIZE) {
            slotError(slot, 'حجم فایل بیشتر از ' + toFa(5) + ' مگابایت است.');
            return;
        }
        slotError(slot, '');
        slot.staged = file;
        $('[data-role="stName"]', slot.root).textContent = file.name;
        $('[data-role="stSize"]', slot.root).textContent = formatSize(file.size);
        setState(slot, 'staged');
    }

    function cancelStage(slot) {
        slot.staged = null;
        slotError(slot, '');
        setState(slot, slot.uploaded ? 'uploaded' : 'empty');
    }

    function startUpload(slot) {
        var file = slot.staged;
        if (!file) return;
        slotError(slot, '');
        $('[data-role="upName"]', slot.root).textContent = file.name;
        var bar = $('.req-progress > span', slot.root);
        var track = $('.req-progress', slot.root);
        var pct = 0;
        bar.style.width = '0%';
        setState(slot, 'uploading');

        // BACKEND HOOK: replace this fake progress with a real upload
        // (fetch / XMLHttpRequest with FormData) and call finish() on success.
        slot.timer = setInterval(function () {
            pct = Math.min(100, pct + 8 + Math.random() * 14);
            bar.style.width = pct + '%';
            track.setAttribute('aria-valuenow', Math.round(pct));
            if (pct >= 100) { clearInterval(slot.timer); slot.timer = null; setTimeout(finish, 180); }
        }, 110);

        function finish() {
            if (slot.uploaded) URL.revokeObjectURL(slot.uploaded.url);
            slot.uploaded = { file: file, url: URL.createObjectURL(file) };
            slot.staged = null;
            renderUploaded(slot);
            setState(slot, 'uploaded');
            announce('فایل «' + slot.def.label + '» آپلود شد.');
        }
    }

    function renderUploaded(slot) {
        var u = slot.uploaded;
        var thumb = $('.req-thumb', slot.root);
        thumb.innerHTML = '';
        if (isImage(u.file)) {
            var img = document.createElement('img');
            img.src = u.url;
            img.alt = 'پیش‌نمایش ' + slot.def.label;
            thumb.appendChild(img);
        } else {
            thumb.innerHTML = icon('file', 'req-ico--lg') + '<span>PDF</span>';
        }
        thumb.insertAdjacentHTML('beforeend', '<span class="req-thumb-badge">' + icon('expand', 'req-ico--sm') + '</span>');
        $('[data-role="doName"]', slot.root).textContent = u.file.name;
        $('[data-role="doSize"]', slot.root).textContent = formatSize(u.file.size);
    }

    function removeFile(slot) {
        if (slot.uploaded) URL.revokeObjectURL(slot.uploaded.url);
        slot.uploaded = null;
        slot.staged = null;
        slotError(slot, '');
        setState(slot, 'empty');
        announce('فایل «' + slot.def.label + '» حذف شد.');
    }

    function destroySlot(slot) {
        if (slot.timer) clearInterval(slot.timer);
        if (slot.uploaded) URL.revokeObjectURL(slot.uploaded.url);
    }

    /* ---------- full-screen viewer ---------- */
    var viewer = $('#viewer');
    var viewerStage = $('#viewerStage');
    var viewerName = $('#viewerName');
    var viewerClose = $('#viewerClose');
    var viewerOpener = null;

    function openViewer(slot, opener) {
        if (!slot.uploaded) return;
        var u = slot.uploaded;
        viewerStage.innerHTML = '';
        if (isImage(u.file)) {
            var img = document.createElement('img');
            img.src = u.url;
            img.alt = slot.def.label;
            viewerStage.appendChild(img);
        } else {
            var fr = document.createElement('iframe');
            fr.src = u.url;
            fr.title = slot.def.label;
            viewerStage.appendChild(fr);
        }
        viewerName.textContent = slot.def.label + ' — ' + u.file.name;
        viewerOpener = opener || null;
        viewer.hidden = false;
        document.documentElement.style.overflow = 'hidden';
        viewerClose.focus();
    }

    function closeViewer() {
        if (viewer.hidden) return;
        viewer.hidden = true;
        viewerStage.innerHTML = '';
        document.documentElement.style.overflow = '';
        if (viewerOpener) viewerOpener.focus();
        viewerOpener = null;
    }

    viewerClose.addEventListener('click', closeViewer);
    viewer.addEventListener('click', function (e) {
        // click on the dark backdrop (not on the image / pdf) also closes
        if (e.target === viewer || e.target === viewerStage) closeViewer();
    });
    document.addEventListener('keydown', function (e) {
        if (viewer.hidden) return;
        if (e.key === 'Escape') { e.preventDefault(); closeViewer(); }
        else if (e.key === 'Tab') { e.preventDefault(); viewerClose.focus(); }   // keep focus inside the viewer
    });

    /* ---------- phone ---------- */
    var defaultPhone = form.dataset.defaultPhone || '';
    $('#defaultPhoneText').textContent = defaultPhone ? toFa(defaultPhone) : '—';

    phoneRadios.forEach(function (r) {
        r.addEventListener('change', function () {
            var isNew = currentPhoneMode() === 'new';
            newPhoneWrap.hidden = !isNew;
            if (isNew) { newPhone.focus(); }
            else { clearError(newPhoneWrap); newPhone.value = ''; }
        });
    });
    function currentPhoneMode() {
        var c = phoneRadios.filter(function (r) { return r.checked; })[0];
        return c ? c.value : 'default';
    }
    newPhone.addEventListener('input', function () {
        newPhone.value = toEn(newPhone.value).replace(/\D/g, '').slice(0, 11);
        clearError(newPhoneWrap);
    });

    /* ---------- notes (auto-growing, optional) ---------- */
    notes.maxLength = NOTES_MAX;
    function growNotes() {
        notes.style.height = 'auto';
        var h = notes.scrollHeight + 2;
        if (h > notes.clientHeight) notes.style.height = Math.min(h, 480) + 'px';
        else notes.style.height = '';
        notesCounter.textContent = toFa(notes.value.length) + ' / ' + toFa(NOTES_MAX);
    }
    notes.addEventListener('input', growNotes);
    growNotes();

    /* ---------- validation ---------- */
    function errorNode() { var p = el('p', 'req-error'); p.hidden = true; return p; }

    function showError(wrap, msg) {
        var p = $('.req-error', wrap);
        if (!p) { p = errorNode(); wrap.appendChild(p); }
        p.innerHTML = icon('alert', 'req-ico--sm') + '<span></span>';
        $('span', p).textContent = msg;
        p.hidden = false;
        var inp = $('.profile-input', wrap);
        if (inp) inp.classList.add('is-invalid');
    }
    function clearError(wrap) {
        if (!wrap) return;
        var p = $('.req-error', wrap);
        if (p) p.hidden = true;
        $$('.profile-input', wrap).forEach(function (i) { i.classList.remove('is-invalid'); });
        hideAlert();
    }
    function hideAlert() { formAlert.hidden = true; }
    function announce(msg) { live.textContent = ''; setTimeout(function () { live.textContent = msg; }, 30); }

    function validate() {
        var firstBad = null;
        function bad(node) { if (!firstBad) firstBad = node; }

        if (!typeSelect.value) {
            showError(typeSelect.closest('.profile-field'), 'لطفاً نوع درخواست را انتخاب کنید.');
            bad(typeSelect);
        }

        $$('[data-field]', dynamicFields).forEach(function (wrap) {
            var f = wrap._def, input = wrap._input;
            var v = toEn(input.value).trim();
            if (f.required && !v) {
                showError(wrap, f.type === 'select' ? 'لطفاً یکی از گزینه‌ها را انتخاب کنید.' : 'این فیلد الزامی است.');
                bad(input);
            } else if (v && f.digits && v.length !== f.digits) {
                showError(wrap, f.patternMsg || 'مقدار واردشده معتبر نیست.');
                bad(input);
            } else if (v && f.type === 'number' && !/^[1-9]\d{0,5}$/.test(v)) {
                showError(wrap, f.patternMsg || 'یک عدد معتبر وارد کنید.');
                bad(input);
            }
        });

        Object.keys(state.slots).forEach(function (k) {
            var s = state.slots[k];
            var st = s.root.dataset.state;
            if (st === 'uploading') { slotError(s, 'لطفاً تا پایان آپلود صبر کنید.'); bad(s.root); }
            else if (st === 'staged') { slotError(s, 'فایل انتخاب‌شده هنوز آپلود نشده است. دکمه‌ی «آپلود» را بزنید.'); bad(s.root); }
            else if (s.def.required && !s.uploaded) { slotError(s, 'آپلود «' + s.def.label + '» الزامی است.'); bad(s.root); }
        });

        if (currentPhoneMode() === 'new') {
            var p = toEn(newPhone.value).trim();
            if (!/^09\d{9}$/.test(p)) {
                showError(newPhoneWrap, 'شماره موبایل باید ۱۱ رقم و با ۰۹ شروع شود.');
                bad(newPhone);
            }
        }

        if (firstBad) {
            formAlert.hidden = false;
            $('span', formAlert).textContent = 'لطفاً موارد مشخص‌شده را تکمیل کنید.';
            firstBad.scrollIntoView({ behavior: 'smooth', block: 'center' });
            if (firstBad.focus && firstBad.tagName !== 'DIV') firstBad.focus({ preventScroll: true });
            return false;
        }
        return true;
    }

    /* ---------- submit (mock) ---------- */
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        if (!validate()) return;

        var data = {
            requestType: state.type.id,
            fields: {},
            files: {},
            phone: currentPhoneMode() === 'new' ? toEn(newPhone.value).trim() : defaultPhone,
            phoneMode: currentPhoneMode(),
            notes: notes.value.trim()
        };
        $$('[data-field]', dynamicFields).forEach(function (w) { data.fields[w._def.name] = toEn(w._input.value).trim(); });
        Object.keys(state.slots).forEach(function (k) {
            var u = state.slots[k].uploaded;
            data.files[k] = u ? { name: u.file.name, size: u.file.size, type: u.file.type } : null;
        });

        submitBtn.disabled = true;
        submitBtn.querySelector('span').textContent = 'در حال ثبت…';

        // BACKEND HOOK: send `data` (and the real File objects) to the server here.
        console.log('[mock] request payload:', data);

        setTimeout(function () {
            $('#reqCode').textContent = 'REQ-' + Math.floor(100000 + Math.random() * 900000);
            form.hidden = true;
            success.hidden = false;
            success.classList.add('req-reveal');
            success.scrollIntoView({ behavior: 'smooth', block: 'center' });
            announce('درخواست شما با موفقیت ثبت شد.');
        }, 900);
    });

    function resetAll() {
        resetDynamic();
        form.reset();
        state.type = null;
        typeHint.hidden = true;
        afterType.hidden = true;
        afterType.classList.remove('req-reveal');
        newPhoneWrap.hidden = true;
        $$('.is-invalid', form).forEach(function (n) { n.classList.remove('is-invalid'); });
        $$('.req-error', form).forEach(function (n) { n.hidden = true; });
        hideAlert();
        growNotes();
        submitBtn.disabled = false;
        submitBtn.querySelector('span').textContent = 'ثبت درخواست';
    }

    $('#resetBtn').addEventListener('click', resetAll);
    $('#newRequestBtn').addEventListener('click', function () {
        success.hidden = true;
        form.hidden = false;
        resetAll();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    window.addEventListener('beforeunload', function () {
        Object.keys(state.slots).forEach(function (k) { destroySlot(state.slots[k]); });
    });
})();
