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

    var S={
        orderID: window.location.href.match(/\/Survey\/(\d+)\.html($|\?)/i)[1],
        vld: new $.Validation(),
        load: function () {
            $.post("/Order/GetSurveyByID/",function (res) {
                if(res.success) {
                    if(res.data) {
                        $$("like").value(res.data.SurveyLike);
                        $("name").value=res.data.CustomerName;
                        $$("gender").value(res.data.Gender?1:0);
                        $("mobile").value=res.data.Mobile;
                        $("email").value=res.data.Email;
                        loadCity(res.data.ProvinceID,res.data.CityID,res.data.RegionID);
                        $("address").value=res.data.Address;
                        $$("type").value(res.data.Type);
                        $$("square").value(res.data.SquareType);

                        res.data.Options=res.data.Options.replace(/,\s+/,",").replace(/,/g,", ");
                        var options=res.data.Options.split(', ');
                        var otherOpt=options[options.length-1];
                        if(!/^\d+$/.test(otherOpt)) {
                            $("otherOptions").value=otherOpt;
                        }
                        $$("options").value(res.data.Options);
                        $$("days").value(res.data.Days);
                        if(!/^\d+$/.test(res.data.HopeTime)) {
                            $("otherTime").value=otherOpt;
                        } else {
                            $$("time").value(res.data.HopeTime);
                        }
                        $$("hour").value(res.data.HopeHour);
                        $("txtMemo").value=res.data.Memo;
                    }
                } else
                    alert(res.msg);

            },{
                orderID: S.orderID
            });
        },
        init: function () {
            S.load();

            S.vld.add("name",{ emptyAble: false,emptyText: "请填写姓名！" });
            S.vld.add("mobile",{ emptyAble: false,emptyText: "请填写手机！" });
            S.vld.add("email",{ emptyAble: false,emptyText: "请填写邮箱！" });
            S.vld.add("cmbRegion",{
                validate: function () {
                    return $("cmbRegion").value!=0;
                },
                validationText: "请选择省市区！"
            });
            S.vld.add("address",{ emptyAble: false,emptyText: "请填写量房地址！" });
            S.vld.add($$("type"),{ emptyAble: false,emptyText: "请选择装修类型！" });
            S.vld.add($$("square"),{ emptyAble: false,emptyText: "请选择面积！" });
            S.vld.add("otherOptions",{
                validate: function () {
                    return $("otherOptions").value||$$("options").value();
                },
                validationText: "请选择计划配置！"
            });
            S.vld.add($$("days"),{ emptyAble: false,emptyText: "请选择希望量房日期！" });
            S.vld.add("otherTime",{
                validate: function () {
                    return $("otherTime").value||$$("time").value();
                },
                validationText: "请选择希望上门时间！"
            });
            S.vld.add($$("hour"),{ emptyAble: false,emptyText: "请选择希望上门时间！" });

            $("surveyBlk").onsubmit=function () {
                var vldRes=S.vld.validate();
                if(!vldRes) {
                    alert("带 * 项必填！");
                    window.scrollTo(0,0);
                }
                return vldRes;
            };
        }
    }

    S.init();
});