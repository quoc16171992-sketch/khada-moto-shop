// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.querySelectorAll('[data-submit-loading]').forEach((form) => {
    form.addEventListener('submit', () => {
        if (!form.checkValidity()) return;
        const button = form.querySelector('[data-submit-button]');
        if (button) {
            button.disabled = true;
            button.textContent = 'Đang xử lý...';
        }
    });
});

document.querySelectorAll('[data-product-gallery]').forEach((gallery) => {
    const mainImage = gallery.querySelector('[data-gallery-main]');
    if (!mainImage) return;

    gallery.querySelectorAll('[data-gallery-src]').forEach((button) => {
        button.addEventListener('click', () => {
            mainImage.src = button.dataset.gallerySrc || mainImage.src;
            mainImage.alt = button.dataset.galleryAlt || mainImage.alt;
            gallery.querySelectorAll('[data-gallery-src]').forEach((item) => item.classList.remove('active'));
            button.classList.add('active');
        });
    });
});
