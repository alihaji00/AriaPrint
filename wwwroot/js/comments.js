(function () {
    "use strict";

    var persianDigits = ["۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹"];
    function toPersianDigits(input) {
        return String(input).replace(/[0-9]/g, function (d) { return persianDigits[d]; });
    }

    // ---- toggle like buttons (calls backend) ----
    function bindLikeButton(btn) {
        if (btn.dataset.bound === "1") return;
        btn.dataset.bound = "1";

        var commentId = btn.dataset.commentId;
        if (!commentId) return;

        var inFlight = false;
        btn.addEventListener("click", function () {
            if (inFlight) return;
            inFlight = true;
            var countEl = btn.querySelector(".qa-like-count");
            var count = parseInt(countEl.getAttribute("data-count") || "0", 10);
            var liked = !btn.classList.contains("qa-like-btn-active");
            var optimisticCount = liked ? count + 1 : Math.max(0, count - 1);

            // optimistic UI
            btn.classList.toggle("qa-like-btn-active", liked);
            countEl.setAttribute("data-count", optimisticCount);
            countEl.textContent = toPersianDigits(optimisticCount);

            var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenInput ? tokenInput.value : "";

            fetch("/Comments/ToggleLike?commentId=" + encodeURIComponent(commentId), {
                method: "POST",
                credentials: "same-origin",
                headers: { "RequestVerificationToken": tokenValue || "" }
            })
                .then(function (r) {
                    if (!r.ok) throw new Error("HTTP " + r.status);
                    return r.json().catch(function () { return null; });
                })
                .then(function (data) {
                    if (data && typeof data.likes === "number") {
                        countEl.setAttribute("data-count", data.likes);
                        countEl.textContent = toPersianDigits(data.likes);
                        if (typeof data.liked === "boolean") {
                            btn.classList.toggle("qa-like-btn-active", data.liked);
                        }
                    }
                })
                .catch(function () {
                    // rollback on failure
                    btn.classList.toggle("qa-like-btn-active", !liked);
                    countEl.setAttribute("data-count", count);
                    countEl.textContent = toPersianDigits(count);
                })
                .finally(function () { inFlight = false; });
        });
    }

    // ---- toggle inline reply box under a question ----
    function bindReplyButton(btn) {
        if (btn.dataset.bound === "1") return;
        btn.dataset.bound = "1";
        btn.addEventListener("click", function () {
            var existing = btn.nextElementSibling;
            if (existing && existing.classList.contains("qa-reply-box")) {
                existing.remove();
                btn.classList.remove("qa-reply-btn-active");
                return;
            }

            btn.classList.add("qa-reply-btn-active");

            var commentId = btn.dataset.questionId;
            var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenInput ? tokenInput.value : "";

            var box = document.createElement("div");
            box.className = "qa-reply-box";
            box.innerHTML =
                '<form method="post" action="/Answers/Answers" class="qa-reply-form qa-reply-box">' +
                '<input type="hidden" name="__RequestVerificationToken" value="' + tokenValue + '">' +
                '<input type="hidden" name="AnswerId" value="' + commentId + '">' +
                '<input name="Text" type="text" class="qa-input qa-reply-input" placeholder="پاسخ خود را بنویسید...">' +
                '<button type="submit" class="qa-reply-send">ارسال</button>' +
                '</form>';



            btn.insertAdjacentElement("afterend", box);

        });
    }

    // ---- delete button (UI confirmation only — wire to backend later) ----
    function bindDeleteButton(btn) {
        if (btn.dataset.bound === "1") return;
        btn.dataset.bound = "1";

        btn.addEventListener("click", function () {
            var commentId = btn.dataset.commentId;
            if (!commentId) return;
            if (!window.confirm("آیا از حذف این دیدگاه مطمئن هستید؟")) return;

            // optimistic UI — fade out the comment article
            var article = btn.closest(".qa-item");
            if (article) {
                article.style.transition = "opacity .3s ease, transform .3s ease";
                article.style.opacity = "0.4";
                article.style.pointerEvents = "none";
            }

            var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenInput ? tokenInput.value : "";

            fetch("/Comments/Delete?commentId=" + encodeURIComponent(commentId), {
                method: "POST",
                credentials: "same-origin",
                headers: { "RequestVerificationToken": tokenValue || "" }
            })
                .then(function (r) {
                    if (r.ok) {
                        if (article) article.remove();
                    } else {
                        throw new Error("HTTP " + r.status);
                    }
                })
                .catch(function () {
                    if (article) {
                        article.style.opacity = "";
                        article.style.pointerEvents = "";
                    }
                    alert("حذف دیدگاه با خطا مواجه شد. لطفاً بعداً تلاش کنید.");
                });
        });
    }

    document.querySelectorAll(".qa-like-btn").forEach(bindLikeButton);
    document.querySelectorAll(".qa-reply-btn").forEach(bindReplyButton);
    document.querySelectorAll(".qa-delete-btn").forEach(bindDeleteButton);

    // ---- show / hide extra questions ----
    var viewAllBtn = document.getElementById("qaViewAllBtn");
    var expanded = false;

    if (viewAllBtn) {
        viewAllBtn.addEventListener("click", function () {
            expanded = !expanded;
            document.querySelectorAll(".qa-item-hidden").forEach(function (el) {
                el.style.display = expanded ? "block" : "none";
            });
            viewAllBtn.querySelector("span").textContent = expanded ? "نمایش کمتر" : "مشاهده همه سوالات";
            viewAllBtn.classList.toggle("qa-viewall-btn-open", expanded);
        });
    }
});