$.ready(function () {
    function formatPrice(price) {
        price=Math.round(parseFloat(price)*100)/100+"";
        return "￥"+price+(/^\d+$/.test(price)?".00":/^\d+\.\d{1}$/.test(price)?"0":"")
    };

    var productGrid=$.Grid({
        id: "productList",
        rowHeight: 70,
        url: "/Order/GetOrderDetails/",
        params: {
            orderId: window.location.href.match(/\/Order\/(\d+)\.html($|\?)/)[1]
        },
        columns: [{
            header: "商品",
            type: "custom",
            width: 80,
            custom: function (cell,data) {
                var link=$("A",true).prop("target","_blank").prop("href","/Product/"+data.ProductID+".html");
                $("IMG",link).prop("src",media+data.Url).css({ width: "80px",height: "60px" });
                cell.append(link);
            }
        },{
            header: "名称",
            type: "custom",
            width: 200,
            custom: function (cell,data) {
                cell.append($("A",true).prop("target","_blank").prop("href","/Product/"+data.ProductID+".html").html(data.Name));
            }
        },{
            header: "货号",
            columnName: "Code",
            width: 60
        },{
            header: "单价",
            type: "custom",
            width: 60,
            custom: function (cell,data) {
                cell.html(formatPrice(data.OrignalPrice));
            }
        },{
            header: "赠送积分",
            columnName: "Points",
            width: 60
        },{
            header: "数量",
            columnName: "Quantity",
            width: 60
        },{
            header: "运费",
            type: "custom",
            width: 60,
            custom: function (cell,data) {
                cell.html("￥"+(data.Freight+(data.Quantity-1)*data.Freight1));
            }
        },{
            width: 60,
            header: "优惠",
            type: "custom",
            custom: function (cell,data) {
                cell.html(data.Discount);
            }
        },{
            header: "小记",
            type: "custom",
            width: 60,
            custom: function (cell,data) {
                cell.html(formatPrice(data.OrignalPrice*data.Quantity-data.Discount));
            }
        },{
            header: "",
            type: "custom",
            width: 60,
            custom: function (cell,data) {
                if(orderStatus==4) {
                    cell.append($("A",true).css("color:#c00").prop("target","_blank").prop("href","/Product/"+data.ProductID+".html#comment").html("[评论]"));
                } else {
                    cell.append($("A",true).prop("target","_blank").prop("href","/Product/"+data.ProductID+".html").html("[查看]"));
                }
            }
        }]
    });

});