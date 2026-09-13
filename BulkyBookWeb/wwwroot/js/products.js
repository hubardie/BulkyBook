new DataTable('#tblData', {
    ajax: '/product/getall',
    columns: [
        { data: 'title' },
        { data: 'isbn' },
        { data: 'price' },
        { data: 'author' },
        { data: 'category.name' },
        { defaultContent: ''}
    ]
});