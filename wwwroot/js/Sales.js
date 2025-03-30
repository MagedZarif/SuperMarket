function loadCategories() {
    fetch("/Sales/GetCategories")
        .then(res => res.json())
        .then(data => {
            let select = document.getElementById("categorySelect");
            select.innerHTML = "<option value=''>Select Category</option>";
            data.forEach(cat => select.add(new Option(cat.name, cat.id)));
        });
}

function loadItems() {
    let categoryId = document.getElementById("categorySelect").value;
    fetch(`/Sales/GetItems?categoryId=${categoryId}`)
        .then(res => res.json())
        .then(data => {
            let select = document.getElementById("itemSelect");
            select.innerHTML = "<option value=''>Select Item</option>";
            data.forEach(item => select.add(new Option(item.name, item.id)));
        });
}

loadCategories();
