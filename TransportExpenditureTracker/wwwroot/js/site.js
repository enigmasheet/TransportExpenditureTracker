// Site-wide utilities
function formatNumber(n) { return parseFloat(n).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
function formatInt(n) { return parseInt(n).toLocaleString('en-IN'); }

// Toast notifications
function showToast(type, message) {
    var iconMap = { success: 'bi-check-circle-fill text-success', error: 'bi-exclamation-circle-fill text-danger', warning: 'bi-exclamation-triangle-fill text-warning', info: 'bi-info-circle-fill text-primary' };
    var icon = iconMap[type] || iconMap.info;
    var toastHtml = '<div class="toast align-items-center border-0" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="4000">'
        + '<div class="d-flex"><div class="toast-body"><i class="bi ' + icon + ' fs-5"></i> '
        + $('<span>').text(message).html()
        + '</div><button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast"></button></div></div>';
    var $toast = $(toastHtml).appendTo('#toastContainer');
    new bootstrap.Toast($toast[0]).show();
    $toast.on('hidden.bs.toast', function () { $(this).remove(); });
}

// Button loading state
$.fn.loading = function (state) {
    if (state) { this.addClass('is-loading').prop('disabled', true); }
    else { this.removeClass('is-loading').prop('disabled', false); }
    return this;
};

$(document).ready(function () {
    // Active nav highlighting
    var currentPath = (window.location.pathname || '').toLowerCase().replace(/\/+$/, '');
    $('.navbar-nav a').each(function () {
        var href = $(this).attr('href');
        if (href && href !== '#' && currentPath === href.toLowerCase().split('?')[0].replace(/\/+$/, '')) {
            $(this).addClass('active');
            if ($(this).closest('.dropdown-menu').length) {
                $(this).closest('.dropdown').find('.dropdown-toggle').addClass('active');
            }
        }
    });

    // Convert TempData to toasts
    var $td = $('#tempData');
    if ($td.length) {
        var success = $td.data('success');
        var error = $td.data('error');
        var warning = $td.data('warning');
        if (success) showToast('success', success);
        if (error) showToast('error', error);
        if (warning) showToast('warning', warning);
    }

    // Auto-loading state on form submits
    $(document).on('submit', 'form', function (e) {
        if (e.isDefaultPrevented()) return;
        var $btn = $(this).find('button[type="submit"]');
        if ($btn.length && !$btn.hasClass('is-loading')) {
            $btn.loading(true);
        }
    });
});
