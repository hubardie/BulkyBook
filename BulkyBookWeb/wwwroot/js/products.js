new DataTable('#tblData', {
    ajax: '/product/getall',
    columns: [
        { data: 'title' },
        { defaultContent: ''}
    ]
});