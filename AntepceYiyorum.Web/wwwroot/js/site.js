// Antepçe Yiyorum - Tarayıcı İçi Etkileşim Betikleri

/**
 * Admin panelinde dosya seçildiğinde ürün fotoğrafını yüklemeden önce canlı önizleme olarak gösterir.
 * @param {HTMLInputElement} input 
 */
function previewProductImage(input) {
    const preview = document.getElementById('imagePreview');
    if (!preview || !input.files || !input.files[0]) {
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        preview.src = e.target.result;
    };
    reader.readAsDataURL(input.files[0]);
}
