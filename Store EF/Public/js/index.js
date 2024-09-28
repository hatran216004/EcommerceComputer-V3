// Hàm để tính toán subtotal
function updateSubtotal(item) {
    const priceElement = item.querySelector('.cart__price-product');
    const quantityInput = item.querySelector('.cart__main-quantity-input');
    const subtotalElement = item.querySelector('.cart__total-product');

    const price = parseFloat(priceElement.innerText);
    const quantity = parseInt(quantityInput.value);
    const subtotal = price * quantity;

    subtotalElement.innerText = subtotal; // Định dạng số với dấu phân cách
    return subtotal; // Trả về subtotal để sử dụng cho tổng giỏ hàng
}

// Hàm để cập nhật tổng giỏ hàng và tổng cộng
function updateCartTotals() {
    let cartSubtotal = 0;
    document.querySelectorAll('.cart__main-items').forEach((item) => {
        cartSubtotal += updateSubtotal(item); // Cộng dồn subtotal cho mỗi mục
    });

    // Cập nhật tổng giỏ hàng
    document.getElementById('cartSubtotal').textContent = cartSubtotal;

    const shippingCostElement = document.getElementById('shippingCost');
    const shippingCost = parseInt(shippingCostElement.innerText); // Lấy giá trị shipping cost từ HTML

    // Cập nhật tổng cộng
    const cartTotal = cartSubtotal + shippingCost;
    document.getElementById('cartTotal').textContent = cartTotal;
}

// Hàm để tăng số lượng
function increaseQuantity(event) {
    const item = event.target.closest('.cart__main-items');
    const quantityInput = item.querySelector('.cart__main-quantity-input');
    quantityInput.value = parseInt(quantityInput.value) + 1; // Tăng số lượng lên 1
    updateCartTotals(); // Cập nhật tổng giỏ hàng
}

// Hàm để giảm số lượng
function decreaseQuantity(event) {
    const item = event.target.closest('.cart__main-items');
    const quantityInput = item.querySelector('.cart__main-quantity-input');
    if (quantityInput.value > 1) {
        quantityInput.value = parseInt(quantityInput.value) - 1; // Giảm số lượng xuống 1
        updateCartTotals(); // Cập nhật tổng giỏ hàng
    }
}

// Gán sự kiện cho nút tăng và giảm số lượng
document.querySelectorAll('.cart-btn-up').forEach((button) => {
    button.addEventListener('click', increaseQuantity);
});

document.querySelectorAll('.cart-btn-down').forEach((button) => {
    button.addEventListener('click', decreaseQuantity);
});

// Cập nhật tổng giỏ hàng khi tải trang
updateCartTotals();
