
    //讀取cookie
var readCookie = function (name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
    //設定cookie
var doCookieSetup = function (name, value) {
    var expires = new Date();
    //有效時間保存 30 分 30*60*1000
    expires.setTime(expires.getTime() + 1800000);
    document.cookie = name + "=" + escape(value) + ";expires=" + expires.toGMTString()
}
    //刪除cookie
var delCookie = function (name) {
    var exp = new Date();
    exp.setTime(exp.getTime() - 1);
    var cval = readCookie(name);
    document.cookie = escape(name) + "=" + cval + "; expires=" + exp.toGMTString();
}
