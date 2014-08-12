function LoginDialog(url) {
    var frg=document.createDocumentFragment();
    var container=$("DIV",frg).css({ "width": "500px" });
    var tabTit=$("UL",$("DIV",container).css("tabTit"));
    var loginTabTit=$("LI",tabTit).html("登录").css("curr").on("click",function () {
        if(this.className!="curr") {
            regTabCon.css({ display: "none" });
            regTabTit.css("");
            this.css("curr");
            loginTabCon.css({ display: "block" });
            dialog.fixPlace();
        }
        changeCheckCode("__login_cc");
    });
    var regTabTit=$("LI",tabTit).html("注册").on("click",function () {
        if(this.className!="curr") {
            loginTabCon.css({ display: "none" });
            loginTabTit.css("");
            this.css("curr");
            regTabCon.css({ display: "block" });
            dialog.fixPlace();
        }
        changeCheckCode("__reg_cc");
    });
    var loginTabCon=$("DIV",container).on("keypress",function (e) {
        if(e.keyCode==13)
            loginBtn.on("click");
    });
    var regTabCon=$("DIV",container).css({ display: "none" }).on("keypress",function (e) {
        if(e.keyCode==13)
            regBtn.on("click");
    });

    var dl=$("DL",loginTabCon).css({ clear: "both","overflow": "hidden","marginTop": "20px" });
    var dt=$("DT",dl).css({ "float": "left","width": "100px","textAlign": "right",color: "#666","lineHeight": "22px" }).html("用户名：");
    var dd=$("DD",dl).css({ "float": "left",color: "#666" });
    var loginName=$("INPUT",dd).css({ "float": "left","border": "1px solid #bdbdbd","height": "20px","lineHeight": "21px","width": "180px" });

    dl=$("DL",loginTabCon).css({ clear: "both","overflow": "hidden","marginTop": "20px" });
    dt=$("DT",dl).css({ "float": "left","width": "100px","textAlign": "right",color: "#666","lineHeight": "22px" }).html("密码：");
    dd=$("DD",dl).css({ "float": "left",color: "#666" });
    var loginPwd=$("PASSWORD",dd).css({ "float": "left","border": "1px solid #bdbdbd","height": "20px","lineHeight": "21px","width": "180px" });
    $("A",dd).css({ "float": "left","margin": "4px 0px 0px 10px",color: "#217371" }).html("忘记密码？");

    dl=$("DL",loginTabCon).css({ clear: "both","overflow": "hidden","marginTop": "20px" });
    dt=$("DT",dl).css({ "float": "left","width": "100px","textAlign": "right",color: "#666","lineHeight": "22px" }).html("验证码：");
    dd=$("DD",dl).css({ "float": "left",color: "#666" });
    var loginCC=$("INPUT",dd).css({ "float": "left","border": "1px solid #bdbdbd","height": "20px","lineHeight": "21px","width": "80px" });
    $("IMG",dd).css({ "float": "left","margin": "0px 0px 0px 10px","cursor": "pointer" }).prop("id","__login_cc").prop("src","/CheckCode/1.jpg").on("click",function () {
        changeCheckCode("__login_cc");
    });

    var buttons=$("DL",loginTabCon).css({ clear: "both","overflow": "hidden","margin": "20px 0px 20px 100px" });
    var loginBtn=$("A",buttons).css("btn").css({ "float": "left" }).on("click",function () {
        if(logVld.validate()) {
            $.post("/User/Login/",function (res) {
                if(res.success)
                    window.location.href=url||window.location.href;
                else
                    alert(res.msg)
            },{
                userName: loginName.value,
                password: loginPwd.value,
                checkCode: loginCC.value
            });
        }

    });
    $("EM",loginBtn).css({ fontSize: "14px","fontWeight": "bold","padding": "0px 15px" }).html("登录");

    $("A",$("SPAN",buttons).html("未注册？").css({ "float": "left","marginLeft": "15px","color": "#999" })).html("立即免费注册").css({ color: "#217371",textDecoration: "underline" }).prop("href","javascript:void(0);").on("click",function () {
        regTabTit.on("click");
    });

    var logVld=new $.Validation();
    logVld.add(loginName,{ emptyAble: false,emptyText: "请填写用户名" });
    logVld.add(loginPwd,{ emptyAble: false,emptyText: "请填写登录密码" });
    logVld.add(loginCC,{ emptyAble: false,emptyText: "请填写验证码" });

    dl=$("DL",regTabCon).css({ clear: "both","overflow": "hidden","marginTop": "20px" });
    dt=$("DT",dl).css({ "float": "left","width": "100px","textAlign": "right",color: "#666","lineHeight": "22px" }).html("用户名：");
    dd=$("DD",dl).css({ "float": "left",color: "#666" });
    var regName=$("INPUT",dd).css({ "float": "left","border": "1px solid #bdbdbd","height": "20px","lineHeight": "21px","width": "180px" });

    dl=$("DL",regTabCon).css({ clear: "both","overflow": "hidden","marginTop": "20px" });
    dt=$("DT",dl).css({ "float": "left","width": "100px","textAlign": "right",color: "#666","lineHeight": "22px" }).html("密码：");
    dd=$("DD",dl).css({ "float": "left",color: "#666" });
    var regPwd=$("PASSWORD",dd).css({ "float": "left","border": "1px solid #bdbdbd","height": "20px","lineHeight": "21px","width": "180px" });

    dl=$("DL",regTabCon).css({ clear: "both","overflow": "hidden","marginTop": "20px" });
    dt=$("DT",dl).css({ "float": "left","width": "100px","textAlign": "right",color: "#666","lineHeight": "22px" }).html("确认密码：");
    dd=$("DD",dl).css({ "float": "left",color: "#666" });
    var regCfmPwd=$("PASSWORD",dd).css({ "float": "left","border": "1px solid #bdbdbd","height": "20px","lineHeight": "21px","width": "180px" });

    dl=$("DL",regTabCon).css({ clear: "both","overflow": "hidden","marginTop": "20px" });
    dt=$("DT",dl).css({ "float": "left","width": "100px","textAlign": "right",color: "#666","lineHeight": "22px" }).html("邮箱：");
    dd=$("DD",dl).css({ "float": "left",color: "#666" });
    var regEmail=$("INPUT",dd).css({ "float": "left","border": "1px solid #bdbdbd","height": "20px","lineHeight": "21px","width": "180px" });

    dl=$("DL",regTabCon).css({ clear: "both","overflow": "hidden","marginTop": "20px" });
    dt=$("DT",dl).css({ "float": "left","width": "100px","textAlign": "right",color: "#666","lineHeight": "22px" }).html("验证码：");
    dd=$("DD",dl).css({ "float": "left",color: "#666" });
    var regCC=$("INPUT",dd).css({ "float": "left","border": "1px solid #bdbdbd","height": "20px","lineHeight": "21px","width": "80px" });
    $("IMG",dd).css({ "float": "left","margin": "0px 0px 0px 10px","cursor": "pointer" }).prop("id","__reg_cc").prop("src","/CheckCode/1.jpg").on("click",function () {
        changeCheckCode("__reg_cc");
    });

    buttons=$("DL",regTabCon).css({ clear: "both","overflow": "hidden","margin": "20px 0px 20px 100px" });
    var regBtn=$("A",buttons).css("btn").css({ "float": "left" }).on("click",function () {
        if(regVld.validate()) {
            $.post("/User/Register/",function (res) {
                if(res.success) {
                    alert("恭喜您，注册成功！");
                    window.location.href=url||window.location.href;
                } else
                    alert(res.msg)
            },{
                userName: regName.value,
                password: regPwd.value,
                email: regEmail.value,
                checkCode: regCC.value
            });
        }
    });
    $("EM",regBtn).css({ fontSize: "14px","fontWeight": "bold","padding": "0px 15px" }).html("注册");

    var regVld=new $.Validation();
    regVld.add(regName,{ msg: "4-20位字符，可由英文、数字及“_”、“-”组成",emptyAble: false,emptyText: "请填写用户名",regex: /^[a-zA-Z0-9]{4,20}$/,regexText: "用户名必须由4-20位英文、数字及“_”、“-”组成",
        success: function () {
            regName.msg.css("loading").html("");
            $.post("/User/IsRegist/",function (res) {
                if(res.success) {
                    if(res.exists)
                        regName.msg.css("err").html("该用户名已被注册！");
                    else
                        regName.msg.css("suc").html("");

                } else
                    regName.msg.css("hid");

            },{
                userName: regName.value
            })
        }
    });
    regVld.add(regPwd,{ msg: "6-16位字符，可由英文、数字及“_”、“-”组成",emptyAble: false,emptyText: "请填写密码",regex: /^[a-zA-Z0-9]{6,20}$/,regexText: "必须由6-16位英文、数字及“_”、“-”组成" });
    regVld.add(regCfmPwd,{ compare: regPwd,compareText: "两次密码不一致！" });
    regVld.add(regEmail,{ emptyAble: false,emptyText: "请填写您的邮箱",regex: /^[\w\._-]+@[\w\._-]+$/,regexText: "邮箱格式不正确" });
    regVld.add(regCC,{ emptyAble: false,emptyText: "请填写验证码" });

    var dialog=new $.Dialog({
        title: "您尚未登录",
        width: 520,
        content: frg
    });
    this.show=function (isReg) {
        if(isReg)
            regTabTit.on("click");
        else
            loginTabTit.on("click");
        dialog.show();
    };
    this.close=function () {
        dialog.close();
    }
};

function changeCheckCode(id) {
    document.getElementById(id).src="/CheckCode/1.jpg?dt="+new Date().getTime();
};

var loginDialog;
function login() {
    if(!loginDialog)
        loginDialog=new LoginDialog();

    loginDialog.show();
};

function register() {
    if(!loginDialog)
        loginDialog=new LoginDialog();

    loginDialog.show(true);
};