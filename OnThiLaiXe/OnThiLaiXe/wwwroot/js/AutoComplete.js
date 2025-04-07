var jq = jQuery.noConflict();
jq(document).ready(function () {
    jq("#SearchCauHoi").autocomplete({
        source: function (request, response) {
            jq.ajax({
                url: "/CauHoi/GetAutocompleteData",
                type: "GET",
                dataType: "json",
                data: { term: request.term },
                success: function (data) {
                    response(jq.map(data, function (item) {
                        return {
                            label: item.label,
                            value: item.label,
                            id: item.value
                        };
                    }));
                },
                error: function () {
                    console.log("Error loading autocomplete data.");
                }
            });
        },
        minLength: 2,
        select: function (event, ui) {
            jq("#SearchCauHoi").val(ui.item.label);
            window.location.href = "/QuanLyCauHoi/Display/" + ui.item.id;
            return false;
        }
    });
});