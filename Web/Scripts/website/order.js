$.ready(function () {
    var addressList,addressData,isAddressOpen=false,provinceData,cityData={},regionData={};

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
    })

    function loadAddress(keepOpen) {
        addressList=null;
        $("addressList").html("");
        $.post("/User/GetAddress/",function (res) {
            if(res.success) {
                addressList=res.data;
            }
            if(!addressList||!addressList.length) {
                addressData=null;
                $("addressCon").css({ "display": "none" });
                openAddress();
            } else {
                $("addressCon").css({ "display": "block" });
                if(!addressData)
                    addressData=addressList[0];

                $.each(addressList,function (item) {
                    if(item.AddressID==addressData.AddressID) {
                        addressData=item;
                    }
                    createAddress(item);
                });
                if(!keepOpen) {
                    initAddressInfo(addressData);
                    closeAddress();
                }
                getCartFreight()
            }
        });
    };

    loadAddress();

    function createAddress(data) {
        var row=$("LI",$("addressList"));
        var radio=$("RADIO",row,"address").on("click",function () {
            initAddressForm(data);
            addressData=data;
        });
        if(addressData&&addressData.AddressID==data.AddressID) {
            radio.checked=true;
            initAddressForm(data);
        }
        $("B",row).html(data.Receiver);
        $("SPAN",row).html(data.ProvinceName+"/"+data.CityName+(data.RegionName?"/"+data.RegionName:"")+"/"+data.Address);
        $("A",row).prop("href","javascript:void(0);").html("[删除]").on("click",function () {
            $.post("/User/DeleteAddress/",function (res) {
                if(res.success) {
                    if(addressData&&addressData.AddressID==data.AddressID)
                        addressData=null;
                    loadAddress(true);
                } else
                    alert(res.msg);
            },{
                addressId: data.AddressID
            })
        });
    };

    function initAddressForm(data) {
        $("txtReceiver").value=data.Receiver||"";
        $("txtAddress").value=data.Address||"";
        $("txtZip").value=data.Zip||"";
        $("txtMobile").value=data.Mobile||"";
        $("txtPhone").value=data.Phone||"";
        loadCity(data.ProvinceID,data.CityID,data.RegionID)
    };

    function initAddressInfo(data) {
        addressData=data;
        $("lblReceiver").html(data.Receiver);
        $("lblAddress").html(data.Address);
        $("lblZip").html(data.Zip);
        $("lblMobile").html(data.Mobile);
        $("lblPhone").html(data.Phone);
        $("lblArea").html(data.ProvinceName+"/"+data.CityName+(data.RegionName?"/"+data.RegionName:""));
    };

    function openAddress() {
        initAddressForm(addressData||{});
        $("addressInfo").className="orderEditor";
        isAddressOpen=true;
        $("change1").html("[关闭]");
    };

    function closeAddress() {
        $("addressInfo").className="orderInfo";
        isAddressOpen=false;
        $("change1").html("[修改]");
    };

    $("change1").on("click",function () {
        if(!addressData) {
            alert("请先保存收货人信息");
            return;
        }
        if(isAddressOpen) closeAddress();
        else openAddress();
    });

    var addrVld=new $.Validation();
    addrVld.add("txtReceiver",{ emptyAble: false,emptyText: "请填写收货人姓名" });
    addrVld.add("txtAddress",{ emptyAble: false,emptyText: "请填写收货地址" });
    addrVld.add("txtZip",{ regex: /^\d{6}$/,regexText: "邮编格式错误" });
    addrVld.add("txtMobile",{ regex: /^1\d{10}$/,regexText: "手机号码格式错误" });
    addrVld.add("txtPhone",{ regex: /^\d+-\d+(,\d+-\d+)*$/,regexText: "固话号码格式错误，应为 “区号-号码”。<br>多个号码用,号隔开" });
    addrVld.add("cmbCity",{ regex: /^[1-9]{1}\d*$/,regexText: "请选择城市" });

    function addAddr(isModify) {
        if(addrVld.validate()) {
            if(!$("txtMobile").value&&!$("txtPhone").value) {
                $("txtMobile").msg.css("err").html("手机或固话必填一项！");
                return false;
            }
            else
                $("txtMobile").msg.css("hid");

            var addrId=isModify&&addressData?addressData.AddressID:0;
            $.post("/User/SaveAddress/",function (res) {
                if(res.success) {
                    loadAddress();
                    getCartFreight();

                } else
                    alert(res.msg);
            },{
                addressId: addrId,
                receiver: $("txtReceiver").value,
                address: $("txtAddress").value,
                zip: $("txtZip").value,
                mobile: $("txtMobile").value,
                phone: $("txtPhone").value,
                cityId: $("cmbCity").value,
                regionId: $("cmbRegion").value
            });
        }
    };

    $("btnAddAddress").on("click",function () {
        addAddr();
    });

    $("btnSaveAddress").on("click",function () {
        addAddr(true);
    });

    var isPayOpen=false,payType=1,memo="";

    function openPay() {
        $("payInfo").className="orderEditor";
        isPayOpen=true;
        $("change2").html("[关闭]");
        $("txtMemo").value=memo;
        $$("payType").value(payType);
    };

    function closePay() {
        $("payInfo").className="orderInfo";
        isPayOpen=false;
        $("change2").html("[修改]");
    };

    closePay();

    $("change2").on("click",function () {
        if(isPayOpen)
            closePay();
        else
            openPay();
    });

    $("btnSavePay").on("click",function () {
        memo=$("txtMemo").value;
        payType=$$("payType").value();
        closePay();

        $("lblPayType").html({ 1: "支付宝",2: "银行转账",3: "电汇"}[payType]);
        $("lblMemo").html(memo.replace("\r\n","<br>").replace("\r","<br>").replace("\n","<br>")||"暂无备注");
    });

    var isExpressOpen=false,expressID=$$("expressID").val();

    function openExpress() {
        $("expressInfo").className="orderEditor";
        isExpressOpen=true;
        $("change3").html("[关闭]");
        if(expressID)
            $$("expressID").value(expressID);
    };
    function closeExpress() {
        $("expressInfo").className="orderInfo";
        isExpressOpen=false;
        $("change3").html("[修改]");
    };

    openExpress();

    $("change3").on("click",function () {
        if(isExpressOpen)
            closeExpress();
        else
            openExpress();
    });

    function getCartFreight() {
        if(1) {
            $.post("/Order/GetCartFreight/",function (res) {
                if(res.success) {
                    $("lblOrderFreight").html(res.totalFreight);
                    $("lblTotalAmount").html(res.totalFreight+orderAmount);
                    res.data.each(function (item,i) {
                        productGrid.rows[i].cells[6].html(item=="-"?"-":("￥"+item));
                    });
                }
            },{
                data: $("?buy"),
                regionID: addressData.RegionID
            });
        }
    }

    $("btnSaveExpress").on("click",function () {
        $$("expressID").each(function (item) {
            if(item.checked) {
                expressID=item.value;
                $("lblExpressName").html($(item).attr("n"));
                closeExpress();
                getCartFreight();
                return false;
            }
        });

    });

    function formatPrice(price) {
        price=price+"";
        return "￥"+price+(/^\d+$/.test(price)?".00":/^\d+\.\d{1}$/.test(price)?"0":"")
    };
    var totalAmount;
    var orderAmount;
    var orderFreight;

    var productGrid=new $.Grid({
        id: "cart",
        rowHeight: 70,
        url: "/Order/GetCartProductList/",
        params: {
            data: $("?buy")
        },
        callback: function (res) {
            if(res.success) {
                totalAmount=0;
                orderAmount=0;
                orderFreight=0;
                $.each(res.data,function (data) {
                    if(data.CartType!=1) {
                        var price=data.SpecialPrice!=0&&data.SpecialPrice<data.Price?data.SpecialPrice:data.Price;
                        orderAmount+=price*data.BuyQty;
                        orderFreight+=data.Freight+(data.BuyQty-1)*data.Freight1;
                    }
                });
                totalAmount=orderAmount+orderFreight;
                $("lblOrderAmount").html(orderAmount);
                $("lblOrderFreight").html("-");
                $("lblTotalAmount").html(totalAmount);
            }
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
                cell.append($("A",true).prop("target","_blank").prop("href",data.CartType==1?("/Package/"+data.PackageID+".html"):("/Product/"+data.ProductID+".html")).html(data.CartType==1?data.PackageName:data.Name));
            }
        },{
            header: "型号",
            columnName: "Code",
            width: 60
        },{
            header: "单价",
            type: "custom",
            width: 60,
            custom: function (cell,data) {
                if(data.CartType!=1) {
                    var price;
                    if(data.SpecialPrice!=0&&data.SpecialPrice<data.Price)
                        price="<del>"+formatPrice(data.Price)+"</del><br><span style='color:#ac0000'>"+formatPrice(data.SpecialPrice)+"</span>";
                    else
                        price=formatPrice(data.Price);

                    cell.html(price);
                } else
                    cell.html("[套餐]");
            }
        },{
            header: "赠送积分",
            type: "custom",
            width: 60,
            custom: function (cell,data) {
                if(data.CartType==1) {
                    cell.html("-");

                } else {
                    cell.html(data.Points);
                }
            }
        },{
            header: "数量",
            columnName: "BuyQty",
            width: 60
        }/*,{
            header: "配送",
            type: "custom",
            width: 50,
            custom: function (cell,data) {
                if(data.Express==0) {
                    cell.html("物流");
                } else {
                    $.each(productGrid.getJsonResult().express,function (item) {
                        if(item.ExpressID==data.Express) {
                            cell.html(item.ExpressName);
                            return false;
                        }
                    });
                }
            }
        }*/,{
            header: "运费",
            type: "custom",
            width: 50,
            custom: function (cell,data) {
                cell.html("-");
            }
        },{
            header: "优惠",
            type: "custom",
            width: 60,
            custom: function (cell,data) {
                cell.html("-");
            }
        },{
            header: "小记",
            type: "custom",
            width: 60,
            custom: function (cell,data) {
                if(data.CartType==1) {
                    cell.html("-");

                } else {
                    var iAmount=data.BuyQty*(data.SpecialPrice==0||data.SpecialPrice>data.Price?data.Price:data.SpecialPrice);
                    cell.html(formatPrice(iAmount));
                }
            }
        }]
    });

    $$("expressID")[0].checked=true;

    $("btnConfirmOrder").on("click",function () {
        if(isAddressOpen) {
            $("message").html("请先保存收货人信息").css("err");
            return;
        }

        //        if(isExpressOpen) {
        //            $("message").html("请先保存物流信息").css("err");
        //            return;
        //        }

        if(isPayOpen) {
            $("message").html("请先保存支付方式及备注").css("err");
            return;
        }

        $("message").css("loading").html("正在保存订单...");

        $.post("/Order/CreateOrder/",function (res) {
            if(res.success) {
                $("message").css("hid");
                window.location.href="/Order/"+res.id+".html?step=success"
            } else
                $("message").css("err").html(res.msg);

        },{
            payType: payType,
            memo: memo,
            receiver: addressData.Receiver,
            address: addressData.Address,
            zip: addressData.Zip,
            mobile: addressData.Mobile,
            phone: addressData.Phone,
            cityId: addressData.CityID,
            regionId: addressData.RegionID,
            expressID: expressID,
            buy: $("?buy")
        });
    });

});