// Hàm để tính toán subtotal
//function updateSubtotal(item) {
//    const priceElement = item.querySelector('.cart__price-product');
//    const quantityInput = item.querySelector('.cart__main-quantity-input');
//    const subtotalElement = item.querySelector('.cart__total-product');

//    const price = parseFloat(priceElement.innerText);
//    const quantity = parseInt(quantityInput.value);
//    const subtotal = price * quantity;

//    subtotalElement.innerText = subtotal; // Định dạng số với dấu phân cách
//    return subtotal; // Trả về subtotal để sử dụng cho tổng giỏ hàng
//}

function updateSubtotal(item) {
    const subtotalElement = item.querySelector('.cart__total-product');

    const price = parseInt(item.querySelector('.cart__main-price>input').value);
    const productId = parseInt(item.querySelector('.cart__main-quantity-input').name)
    const quantity = parseInt(item.querySelector('.cart__main-quantity-input').value);
    const subtotal = price * quantity;
    subtotalElement.innerText = new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(subtotal);
    return subtotal;
}

// Hàm để cập nhật tổng giỏ hàng và tổng cộng
function updateCartTotals() {
    let cartSubtotal = 0;
    document.querySelectorAll('.cart__main-items').forEach((item) => {
        cartSubtotal += updateSubtotal(item); // Cộng dồn subtotal cho mỗi mục
    });

    // Cập nhật tổng giỏ hàng
    document.getElementById('cartSubtotal').textContent = new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(cartSubtotal);

    //const shippingCostElement = document.getElementById('shippingCost');
    //const shippingCost = parseInt(shippingCostElement.value); // Lấy giá trị shipping cost từ HTML

    // Cập nhật tổng cộng
    //const cartTotal = cartSubtotal + shippingCost;
    //const cartTotal = cartSubtotal;
    //document.getElementById('cartTotal').innerText = cartTotal
}

// Hàm để tăng số lượng
function increaseQuantity(event) {
    const item = event.target.closest('.cart__main-items');
    const productId = parseInt(item.querySelector('.cart__main-quantity-input').name)
    const quantityInput = item.querySelector('.cart__main-quantity-input');
    quantityInput.value = parseInt(quantityInput.value) + 1; // Tăng số lượng lên 1
    updateCartTotals(); // Cập nhật tổng giỏ hàng

    const formData = new FormData();
    formData.append('product', productId);
    formData.append('quantity', quantityInput.value);
    try {
        fetch("/Cart/Add", {
            method: "POST",
            body: formData,
        });
    } catch (e) {
        console.error(e);
    }
}

// Hàm để giảm số lượng
function decreaseQuantity(event) {
    const item = event.target.closest('.cart__main-items');
    const productId = parseInt(item.querySelector('.cart__main-quantity-input').name)
    const quantityInput = item.querySelector('.cart__main-quantity-input');
    if (quantityInput.value > 1) {
        quantityInput.value = parseInt(quantityInput.value) - 1; // Giảm số lượng xuống 1
        updateCartTotals(); // Cập nhật tổng giỏ hàng
    }

    const formData = new FormData();
    formData.append('product', productId);
    formData.append('quantity', quantityInput.value);
    try {
        fetch("/Cart/Add", {
            method: "POST",
            body: formData,
        });
    } catch (e) {
        console.error(e);
    }
}

function updateQuantity(event) {
    const item = event.target.closest('.cart__main-items');
    const productId = parseInt(item.querySelector('.cart__main-quantity-input').name)
    const quantityInput = item.querySelector('.cart__main-quantity-input');
    if (quantityInput.value > 1) {
        updateCartTotals(); // Cập nhật tổng giỏ hàng
    } else {
        quantityInput.value = 1
        updateCartTotals();
    }

    const formData = new FormData();
    formData.append('product', productId);
    formData.append('quantity', quantityInput.value);
    try {
        fetch("/Cart/Add", {
            method: "POST",
            body: formData,
        });
    } catch (e) {
        console.error(e);
    }
}

document.querySelectorAll('.cart__main-quantity-input').forEach((input) => {
    input.addEventListener('change', updateQuantity)
});

// Gán sự kiện cho nút tăng và giảm số lượng
document.querySelectorAll('.cart-btn-up').forEach((button) => {
    button.addEventListener('click', increaseQuantity);
});

document.querySelectorAll('.cart-btn-down').forEach((button) => {
    button.addEventListener('click', decreaseQuantity);
});

// Cập nhật tổng giỏ hàng khi tải trang
updateCartTotals();


