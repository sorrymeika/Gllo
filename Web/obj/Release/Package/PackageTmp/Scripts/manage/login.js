function changeCode(id) {
    //刷新验证码
    var rd=Math.random();
    document.getElementById(id).src="/CheckCode/"+Math.round(rd*10)+".jpg";
}
$.ready(function () {
    $("checkCode").msg=$("msgCode");

    var valid=new $.Validation();
    valid.add("checkCode",{
        emptyAble: false,
        emptyText: "请填写验证码"
    });

    valid.add("userName",{
        emptyAble: false,
        emptyText: "请填写用户名"
    });

    valid.add("password",{
        emptyAble: false,
        emptyText: "请填写密码"
    });

    $("formLogin").onsubmit=function () {
        return valid.validate();
    }

    function showInCenter() {
        var rect=$.getDocRect();

        $("login").style.marginTop=Math.max(0,(rect.clientHeight-$("login").offsetHeight)/2)+"px";
    }

    showInCenter();

    window.onresize=function () {
        showInCenter();
    }

});