$.ready(function () {
    var provinceData,cityData={},regionData={};

    $.post("/Area/GetProvince/",function (res) {
        if(res.success&&res.data) {
            provinceData=res.data;
            $.each(provinceData,function (item) {
                $("cmbProvince").options.add(new Option(item.ProvinceName,item.ProvinceID));
            });
        }
    },null,false);

    function loadCity(provinceId,cityId,regionId) {
        for(var i=$("cmbCity").options.length-1;i>0;i--)
            $("cmbCity").options[i]=null;
        $("cmbProvince").value=provinceId;
        if(provinceId!=0) {
            if(!cityData[provinceId]) {
                $.post("/Area/GetCity/",function (res) {
                    if(res.success&&res.data) {
                        cityData[provinceId]=res.data;
                        $.each(res.data,function (item) {
                            $("cmbCity").options.add(new Option(item.CityName,item.CityID));
                        });
                    }
                    $("cmbCity").value=cityId||0;
                    loadRegion(cityId,regionId);
                },{
                    provinceId: provinceId
                });
            } else {
                $.each(cityData[provinceId],function (item) {
                    $("cmbCity").options.add(new Option(item.CityName,item.CityID));
                });
                $("cmbCity").value=cityId||0;
                loadRegion(cityId,regionId);
            }
        }
        else
            loadRegion(cityId,regionId);
    }

    function loadRegion(cityId,regionId) {
        for(var i=$("cmbRegion").options.length-1;i>0;i--)
            $("cmbRegion").options[i]=null;

        if(cityId!=0) {
            if(!regionData[cityId]) {
                $.post("/Area/GetRegion/",function (res) {
                    if(res.success&&res.data) {
                        regionData[cityId]=res.data;
                        $.each(res.data,function (item) {
                            $("cmbRegion").options.add(new Option(item.RegionName,item.RegionID));
                        });
                    }
                    $("cmbRegion").value=regionId||0;
                },{
                    cityId: cityId
                });
            } else {
                $.each(regionData[cityId],function (item) {
                    $("cmbRegion").options.add(new Option(item.RegionName,item.RegionID));
                });
                $("cmbRegion").value=regionId||0;
            }
        }
    }

    $("cmbProvince").on("change",function () {
        loadCity(this.value);
    });

    $("cmbCity").on("change",function () {
        loadRegion(this.value);
    });

    loadCity(provinceID,cityID,regionID);


    var R={
        exchangeID: window.location.href.match(/\/UserExchange\/(\d+)\.html($|\?)/i)[1],
        orderID: orderID,
        vld: new $.Validation(),
        vld1: new $.Validation(),
        init: function () {
            R.vld.add("cmbRegion",{
                validate: function () {
                    return $("cmbRegion").value!=0;
                },
                validationText: "请选择省市区！"
            });
            R.vld.add("reason",{ emptyAble: false,emptyText: "请填写退货原因！" });
            R.vld.add("address",{ emptyAble: false,emptyText: "请填写地址！" });
            R.vld.add("returnProductCode",{ emptyAble: false,emptyText: "请填写退货商品！",
                success: function () {
                    $.post("/Order/CheckReturn/",function (res) {
                        if(res.success) {
                            $("returnProductCode").msg.css("hid").html("");
                            $("returnProductQty").msg.css("hid").html("");
                            if($("returnProductQty").value=="") {
                                $("returnProductQty").value=res.qty||1;
                            }
                            $("returnAmount").html(res.amount);

                        } else
                            $("returnProductCode").msg.css("err").html(res.msg);
                    },{
                        orderID: R.orderID,
                        returnProductCode: $("returnProductCode").value,
                        returnProductQty: $("returnProductQty").value
                    });
                }
            });
            R.vld.add("returnProductQty",{ emptyAble: false,emptyText: "请填写退货数量！",
                success: function () {
                    $.post("/Order/CheckReturn/",function (res) {
                        if(res.success) {
                            $("returnProductCode").msg.css("hid").html("");
                            $("returnProductQty").msg.css("hid").html("");
                        } else
                            $("returnProductQty").msg.css("err").html(res.msg);
                    },{
                        orderID: R.orderID,
                        returnProductCode: $("returnProductCode").value,
                        returnProductQty: $("returnProductQty").value
                    });
                }
            });

            R.vld1.add("exchageProductCode",{
                emptyAble: false,
                emptyText: "请填写换货商品！",
                success: function () {
                    $.post("/Order/CheckExchange/",function (res) {
                        if(res.success) {
                            $("exchageProductQty").msg.css("hid").html("");
                            $("exchageProductCode").msg.css("hid").html("");
                            $("exchangeAmount").html(res.amount);
                        } else
                            $("exchageProductCode").msg.css("err").html(res.msg);
                    },{
                        orderID: R.orderID,
                        exchageProductCode: $("exchageProductCode").value,
                        exchageProductQty: $("exchageProductQty").value
                    });
                }
            });

            R.vld1.add("exchageProductQty",{
                emptyAble: false,
                emptyText: "请填写换货商品数量！",
                success: function () {
                    $.post("/Order/CheckExchange/",function (res) {
                        if(res.success) {
                            $("exchageProductQty").msg.css("hid").html("");
                            $("exchageProductCode").msg.css("hid").html("");
                            $("exchangeAmount").html(res.amount);
                        } else
                            $("exchageProductQty").msg.css("err").html(res.msg);
                    },{
                        orderID: R.orderID,
                        exchageProductCode: $("exchageProductCode").value,
                        exchageProductQty: $("exchageProductQty").value
                    });
                }
            });

            $$("type").on("click",function () {
                if($$("type").val()==0) {
                    $("exc0").css({ "display": "" });
                    $("exc1").css({ "display": "" });
                } else {
                    $("exc0").css({ "display": "none" });
                    $("exc1").css({ "display": "none" });
                }

            });

            $("submit").on("click",function () {
                if(R.vld.validate()) {
                    if($$("type").val()==0) {
                        if(R.vld1.validate()) {

                            $.post("/Order/ModifyReturnExchange/",function (res) {
                                if(res.success) {
                                    alert("修改成功！");
                                    window.location.href="/User/ReturnExchangeList.html"

                                } else
                                    alert(res.msg);

                            },{
                                exchangeID: R.exchangeID,
                                orderID: R.orderID,
                                mobile: $("mobile").value,
                                phone: $("phone").value,
                                type: $$("type").val(),
                                regionID: $("cmbRegion").value,
                                reason: $("reason").value,
                                address: $("address").value,
                                returnProductCode: $("returnProductCode").value,
                                returnProductQty: $("returnProductQty").value,
                                exchageProductCode: $("exchageProductCode").value,
                                exchageProductQty: $("exchageProductQty").value
                            });


                        }
                    } else {

                        $.post("/Order/ModifyReturnExchange/",function (res) {
                            if(res.success) {
                                alert("修改成功！");
                                window.location.href="/User/ReturnExchangeList.html"

                            } else
                                alert(res.msg);

                        },{
                            exchangeID: R.exchangeID,
                            orderID: R.orderID,
                            mobile: $("mobile").value,
                            phone: $("phone").value,
                            type: $$("type").val(),
                            regionID: $("cmbRegion").value,
                            reason: $("reason").value,
                            address: $("address").value,
                            returnProductCode: $("returnProductCode").value,
                            returnProductQty: $("returnProductQty").value,
                            exchageProductCode: "",
                            exchageProductQty: 0
                        });
                    }
                }

            });
        }
    };

    R.init();

});