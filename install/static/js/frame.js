$M.Control["Thumbnail"] = function (BoxID, S) {
    var T = this;
    var A = $("<div style='overflow:auto'/>").appendTo(BoxID);
    T.items = [];
    var columnCount = 6;
    if (S.columnCount) columnCount = S.columnCount;
    var size = 12 / columnCount;
    var item = function (json) {
        var T2 = this;
        var picbox = $("<div class=\"col-xs-" + size + " col-md-" + size + "\" ><a href=\"#\" class=\"thumbnail\"  style='height:" + S.picHeight + "px'  ><img src='" + json.url + "' ></a></div>").appendTo(A);
        T2.attr = function (a, b) {
            if (b != null) json[a] = b;
            return json[a];
        };
        picbox.click(function () {
            if (S.onSelectionChanaged) S.onSelectionChanaged(T, { item: T2 });
        });
    };
    T.addItem = function (json) {
        var obj = new item(json);
        T.items[T.items.length] = obj;
        return obj;
    };
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    if (S.style) A.css(S.style);
};
$M.Control["Popover"] = function (BoxID, S) {
    var T = this;
    var jiantou = "";
    var A = $("<div class=\"popover fade in\" tabindex=\"-1\" style=\"z-index:" + ($M.zIndex + 1) + "\"  ></div>").appendTo($(document.body));
    var B = $("<div class=\"arrow\" ></div>").appendTo(A);
    var title = null;
    if (S.title) title = $("<h3 class=\"popover-title\" ></h3>").appendTo(A);
    var content = $("<div class=\"popover-content\"></div>").appendTo(A);
    T.container = A;
    A.css({"max-width":"1000px"});
    $M.BaseClass.apply(T, [S]);
    T.show = function (obj) {
        var x1 = obj.offset().left, y1 = obj.offset().top, w1 = obj.width(), h1 = obj.height();
        var x2 = A.offset().left, y2 = A.offset().top, w2 = A.width(), h2 = A.height();
        var pageWidth = $(window).width(), pageHeight = $(window).height();
        if (jiantou != "") A.removeClass(jiantou);
        var x = 0, y = 0;
        if (S.location == null) {
            if (y1 > h2) { jiantou = "top"; }
            else if ((pageWidth - x1 - w1) > w2) { jiantou = "right"; }
            else if ((pageHeight - y1 - h1) > h2) { jiantou = "bottom"; }
            else if (x1 > w2) { jiantou = "left"; }
        } else {
            jiantou = S.location;
        }
        switch (jiantou) {
            case "top":
                x = x1; y = y1 - h2; B.css({ left: w1 / 2 + "px", top: "" });
                break;
            case "right":
                x = x1 + w1; y = y1; B.css({ top: h1 / 2 + "px", left: "" });
                break;
            case "bottom":
                x = x1; y = y1 + h1; B.css({ left: w1 / 2 + "px", top: "" });
                break;
            case "right":
                x = x1 - h2; y = y1; B.css({ top: h1 / 2 + "px", left: "" });
                break;
        }
        if (jiantou != "") A.addClass(jiantou);
        //$M.lock(A, true, T, false);
        A.css({ left: x + "px", top: y + "px" });
        A.show();
        $M.focusElement = A[0];
    };
    T.append = function (str) {
        return $(str).appendTo(content);
    };
    T.addControl = function (S2) {
        return content.addControl(S2);
    };
    T.close = function () {
        A.hide();
    };
    T.dispose = function () {
        $(document).unbind("keydown", keydown);
        $(document).unbind("mousedown", mousedown);
        A.remove();
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) $M.focusElement = null;
        A = null;
    };
    T.loseFocus = function () {
        T.remove();
    };
    var keydown = function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {
            if (e.which == 27) {
                T.remove();
            }
            if (S.onKeyDown) S.onKeyDown(T, e);
        }
    };
    var mousedown = function (e) {
        if (A.has(e.target).length == 0) T.close();
    };
    $(document).on("keydown", keydown);
    $(document).on("mousedown", mousedown);
    if (S.style) A.css(S.style);
};
$M.Control["ToolBar"] = function (BoxID, S) {
    var T = this;
    T.controls = [];
    var A = $("<div class=\"btn-toolbar\" ></div>").appendTo(BoxID);
    for (var i = 0; i < S.items.length; i++) {
        if (S.items[i].length != null) {
            var group = $("<div class=\"btn-group\"></div>").appendTo(A);
            for (var i1 = 0; i1 < S.items[i].length; i1++) {
                if (!S.items[i][i1].xtype) S.items[i][i1].xtype = "Button";
                if (S.size != null) S.items[i][i1].size = S["size"];
                T.controls[T.controls.length] = group.addControl(S.items[i][i1]);
            }
        } else {
            if (!S.items[i].xtype) S.items[i].xtype = "Button";
            if (S.size != null) S.items[i].size = S["size"];
            T.controls[T.controls.length] = A.addControl(S.items[i]);
        }
    }
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    if (S["class"]) A.addClass(S["class"]);
};
$M.Control["Form"] = function (BoxID, S, CID) {
    var T = this;
    T.items = new Array();
    var A = $("<form></form>").appendTo(BoxID);
    if (S.url == null) S.url = "";
    T.container = A;
    //A.addClass("form-horizontal");
    $M.BaseClass.apply(T, [S]);
    if (S["class"]) A.addClass(S["class"]);
    var data = {};
    var getData = function (obj) {
        if (obj.controls == null) return;
        for (var i = 0; i < obj.controls.length; i++) {
            if (obj.controls[i] != null) {
                if (obj.controls[i] && obj.controls[i].attr) {
                    var commit = obj.controls[i].attr("commit");
                    var name = obj.controls[i].attr("name");
                    if (name != null && commit != false) {
                        data[name] = obj.controls[i].val();
                    }
                }
                getData(obj.controls[i]);
            }
        }
    };
    var submitForm = function () {
        if (S.onBeginSubmit) S.onBeginSubmit();
        data = {};
        var list = A.find("input");

        for (var i = 0; i < list.length; i++) {
            if ($(list[i]).attr("type") == "file") {
                data[$(list[i]).attr("name")] = $(list[i])[0].files[0];
            } else {
                data[$(list[i]).attr("name")] = $(list[i]).val();
            }
        }
        list = A.find("select");
        for (var i = 0; i < list.length; i++) {
            data[$(list[i]).attr("name")] = $(list[i]).val();
        }
        list = A.find("textarea");
        for (var i = 0; i < list.length; i++) {
            data[$(list[i]).attr("name")] = $(list[i]).val();
        }

        getData(T);
        if (S.command || S.url) {
            if (S.command) {
                $M.comm(S.command, data, function (userData) { if (S.onSubmit) S.onSubmit(T, { "formData": data, "returnData": userData }); }, S.onSubmitErr);
            } else {
                $M.ajax(S.url, data, function (userData) { if (S.onSubmit) S.onSubmit(T, { "formData": data, "returnData": userData }); });
            }
        } else {
            S.onSubmit(T, { "formData": data });
        }
    };
    A.validate({
        //        errorPlacement: function (error, element) {
        //            element.siblings(".help-block").html(error.html());
        //        },
        errorClass: "help-block",
        errorElement: "span",
        highlight: function (element) {
            $(element).closest('div').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('div').removeClass("has-error");

        },
        submitHandler: submitForm
    });

    T.submit = function () {
        A.submit();
    };
    T.find = function (name) {
        if (T.controls) {
            for (var i = 0; i < T.controls.length; i++) {
                if (T.controls[i].attr("name") == name) {
                    return (T.controls[i]);
                }
            }
        }
        return null;
    };
    T.append = function (str) {
        return ($(str).appendTo(A));
    };
    T.val = function (value) {
        var list = A.find("input");
        for (var i = 0; i < list.length; i++) {
            var obj = $(list[i]);
            var name = obj.attr("name");
            if (obj.attr("type") == "checkbox" || obj.attr("type") == "radio") {
                var v = value[name] + "";
                if (v != null) {
                    var vl = v.split(',');
                    obj.prop("checked", vl.indexOf(obj.val()) > -1);
                }
            } else if (obj.attr("type") == "file") {
                
            } else {
                $(list[i]).val(value[name]);
            }
        }
        list = A.find("select");
        for (var i = 0; i < list.length; i++) {
            $(list[i]).val(value[$(list[i]).attr("name")]);
        }
        list = A.find("textarea");
        for (var i = 0; i < list.length; i++) {
            $(list[i]).val(value[$(list[i]).attr("name")]);
        }
        if (T.controls) {
            for (var i = 0; i < T.controls.length; i++) {
                if (T.controls[i].val) T.controls[i].val(value[T.controls[i].attr("name")]);
            }
        }
    };
    return;
    if (S == null) S = {};
    var T = this;
    var A = null;
    if (CID != null) A = CID;
    else {
        A = BoxID.addDom("Form");
    }
    var submitB = null;
    T.items = {};
    var buttons = new Array();
    var inputs = new Array();
    var labels = new Array();
    var IValue = new Array();
    var LValue = new Array();
    T.addDom = function (S2) {
        return (A.addDom(S2));
    };
    if (S.method != null) A.method = S.method;
    T.setAction = function (a) {
        A.action = a;
    };
    if (S.action != null) T.setAction(S.action);
    T.check = function () {
        var err = false;
        for (var i = 0; i < inputs.length; i++) {
            if (inputs[i].check != null) inputs[i].check();
            if (inputs[i].err) err = true
        }
        return (err)
    };
    T.getObj = function (name) {
        for (var i = 0; i < inputs.length; i++) {
            if (inputs[i].name == name) {
                return (inputs[i]);
            }
        }
    };
    T.build = function () {
        var arr = A.getElementsByTagName("div");
        for (var i = 0; i < arr.length; i++) {
            var ctype = null;
            if (arr[i].attributes['ctype'] != null) ctype = arr[i].attributes['ctype'].nodeValue;
            if (ctype == "Control") {
                arr[i].innerHTML = "";
                if (arr[i].items != null) arr[i].items = arr[i].items;
                T.control(arr[i]);
            }
        }
    };
    T.submit = function () {
        var R = false;
        if (!T.check()) {
            if (S.url == null) {
                if (S.submit != null) R = S.submit(T.val());
                else {
                    if (S.onsubmit != null) R = S.onsubmit(T.val());
                }
            } else {
                var A = new $.ajax();
                A.url = S.url;
                A.param = T.val();
                A.type = "post2";
                A.callErr = S.callErr;
                A.callBack = S.callBack;
                A.exec();
            }
            if (R == null) R = false;
        } else {
            R = false;
        }
        return (R)
    };
    T.control = function (obj) {
        obj = $.Function.applyObj(obj);
        var S = {};
        for (var i = 0; i < obj.attributes.length; i++) {
            if (obj.attributes[i].name == "items") {
                S[obj.attributes[i].name] = eval(obj.attributes[i].nodeValue);
            } else {
                S[obj.attributes[i].name] = obj.attributes[i].nodeValue;
            }
        }
        if (S.maxlen != null) S.maxLen = S.maxlen;
        if (S.minlen != null) S.minLen = S.minlen;
        S.width = obj.offsetWidth;
        if (obj.offsetHeight > 25) S.height = obj.offsetHeight;

        $.Index++;
        var M = new $M.Control[S.xtype](null, S, obj);
        if (S.xtype == "Button") buttons[buttons.length] = M;
        else if (S.xtype == "Label") {
            labels[labels.length] = M;
            LValue[LValue.length] = S;
            if (M.name != null) T.items[M.name] = M;
        } else {
            inputs[inputs.length] = M;
            IValue[IValue.length] = S;
            if (M.name != null) T.items[M.name] = M;
        }
        if (S.type == "submit") submitB = M;
        return (M);
    };
    T.addControl = function (S2) {
        var M = A.addControl(S2);
        if (S2.xtype == "Button") buttons[buttons.length] = M;
        else if (S2.xtype == "Label") {
            labels[labels.length] = M;
            LValue[LValue.length] = S2;
            if (M.name != null) T.items[M.name] = M;
        } else {
            inputs[inputs.length] = M;
            IValue[IValue.length] = S2;
            if (M.name != null) T.items[M.name] = M;
        }
        if (S2.type == "submit") submitB = M;
        return (M);
    };
    T.addFormControl = function (S2) {
        var M = A.addFormControl(S2);
        if (M.EditBox != null) {
            inputs[inputs.length] = M.EditBox;
            if (M.name != null) T.items[M.name] = M.EditBox;
        }
        return (M)
    };
    T.val = function () {
        var value = new Array();
        var i1 = 0;
        for (var i = 0; i < inputs.length; i++) {
            if (inputs[i].name != null) {
                if (inputs[i].val != null) {
                    if (S.url == null) {
                        value[i1] = [inputs[i].name, inputs[i].val()];
                    } else {
                        value[i1] = {
                            name: inputs[i].name,
                            value: inputs[i].val()
                        }
                    }
                }
                i1++;
            }
        }
        return (value);
    };
    T.field = function (xml, name) {
        for (var i1 = 0; i1 < xml.length; i1++) {
            var fieldName = xml[i1].getAttribute("name").toLowerCase();
            if (name != null && fieldName == name.toLowerCase()) {
                return (xml[i1].text);
            }
        }
        return ("")
    };
    T.setValue = function (xml) {
        if (xml.length > 0 && xml[0].getAttribute != null) {
            for (var i = 0; i < inputs.length; i++) {
                for (var i1 = 0; i1 < xml.length; i1++) {
                    var fieldName = xml[i1].getAttribute("name").toLowerCase();
                    if (inputs[i].name != null && (fieldName == inputs[i].name.toLowerCase() || (fieldName == "classid" && inputs[i].name.toLowerCase() == "sys_classid"))) {
                        inputs[i].setValue(xml[i1].text);
                    }
                }
            }
            for (var i = 0; i < labels.length; i++) {
                for (var i1 = 0; i1 < xml.length; i1++) {
                    if (labels[i].name != null && xml[i1].getAttribute("name").toLowerCase() == labels[i].name.toLowerCase()) {
                        labels[i].setValue(xml[i1].text);
                    }
                }
            }
        } else {
            setJsonValue(xml);
        }
    };
    var setJsonValue = function (json) {
        for (var i = 0; i < inputs.length; i++) {
            for (var i1 = 0; i1 < json.length; i1++) {
                var fieldName = json[i1].name.toLowerCase();
                if (inputs[i].name != null && (fieldName == inputs[i].name.toLowerCase() || (fieldName == "classid" && inputs[i].name.toLowerCase() == "sys_classid"))) {
                    inputs[i].setValue(json[i1].value);
                }
            }
        }
        for (var i = 0; i < labels.length; i++) {
            for (var i1 = 0; i1 < json.length; i1++) {
                if (labels[i].name != null && json[i1].name.toLowerCase() == labels[i].name.toLowerCase()) {
                    labels[i].setValue(json[i1].value);
                }
            }
        }
    };
    T.reset = function () {
        for (var i = 0; i < inputs.length; i++) {
            if (IValue[i]["value"] != null) inputs[i].setValue(IValue[i]["value"]);
        }
        for (var i = 0; i < labels.length; i++) {
            if (LValue[i]["value"] != null) labels[i].setValue(LValue[i]["value"]);
        }
    };
    A.onsubmit = function () {
        return (T.submit());
    };
    T.setAttribute = function (a, b) {
        S[a] = b;
    }
};
$M.Control["Frame"] = function (BoxID, S) {
    //console.log("框架改变");
    var T = this;
    T.items = new Array();
    var A = null;
    var lineSize = 10;
    if (BoxID[0] == $('body')[0]) {
        A = BoxID;

    }
    else {
        A = BoxID;
    }
    A.addClass("M5_Frame");
    var lines = new Array();
    var item = function (S2) {
        var T2 = this;
        T.items[T.items.length] = T2;
        //var subControl = [];
        var box = null, content = null, head = null, contentDiv = null, title = null;
        if (S2.text) {
            var html = "<div class='box w-box' ><div class=\"w-box-header\">";
            html += "";
            if (S2.ico) html += "<i class=\"fa " + S2.ico + "\"></i> ";
            html += "<span class=\"w-box-title\">" + S2.text + "</span>";
            html += "<span class=\"w-box-options\"></span>";
            html += "";
            html += "</div><div class=\"w-box-content\" style=\"overflow:auto\"></div></div>";
            //    box = $("<div class='box w-box' ><div class=\"w-box-header\">" + S2.text + "</div><div class=\"w-box-content\"></div></div>").appendTo(A);
            box = $(html).appendTo(A);
            head = box.find(".w-box-header");
            title = box.find(".w-box-title");
            if (S2.buttons) {
                var options = box.find(".w-box-options");
                for (var i = 0; i < S2.buttons.length; i++) {
                    //var button=$("<a href='#' class='fa " + S2.buttons[i].ico + "'></a>").appendTo(options);
                    S2.buttons[i].xtype = "Button";
                    S2.buttons[i].size = 0;
                    options.addControl(S2.buttons[i]);
                    //button.click(S2.buttons[i].onClick);
                }
            }
            content = box.find(".w-box-content");
            contentDiv = content;
            //if(S2.items)
            //var t1 = $("<i class='fa fa-chevron-down'></i>").appendTo(head);
            //t1.addControl({ xtype: "Button", ico: "fa-mobile" });
        } else {
            box = $("<div class='box' />").appendTo(A);
            contentDiv = box;
        }
        if (S2.visible == false) box.hide();
        contentDiv = content ? content : box;
        T2.container = contentDiv;
        $M.BaseClass.apply(T2, [S2]);
        T2.attr = function (a, b) {
            if (a == "visible") {
                if (b != null) {
                    S2[a] = b;
                    if (b) { box.show(); } else { box.hide(); }
                    T.resize();
                }
            }
            if (b != null) {
                S2[a] = b;
                if (a == "text" && title) title.html(b);
            }
            return (S[a]);
        };
        T2.css = function (style) {
            box.css(style);
            if (content) {
                content.css({ height: box.height() - head.height() + "px" });
            }
            resize();
        };
        if (S2["class"]) box.addClass(S2["class"]);
        if (S2.style) contentDiv.css(S2.style);
        var resize = function () {
            //alert(T2.controls);
            var w = contentDiv.width(), h = contentDiv.height();
            if (T2.controls) {
                //console.log(T2.controls.length);
                var countHeight = 0;
                var dockC = [];
                for (var i = 0; i < T2.controls.length; i++) {
                    if (T2.controls[i].attr("dock") == 2) {
                        dockC[dockC.length] = T2.controls[i];
                    } else {
                        //try{
                        countHeight += T2.controls[i].height();
                        //}catch(x){
                        //   alert(T2.controls[i].attr("xtype")); 
                        //}
                    }
                }
                if (dockC.length > 0) {

                    dockC[0].css({ width: w + "px", height: (h - countHeight) + "px" });
                }
            }
        };
        T2.box = box;
    };
    for (var i = 0; i < S.items.length; i++) {
        if (i > 0) lines[i] = $("<div class='line' />").appendTo(A);
        new item(S.items[i]);
    };
    var getAutoSize = function (maxSize) {
        var autoCount = 0;
        var width = 0;
        for (var i = 0; i < S.items.length; i++) {
            if (typeof (S.items[i].size) == "number") {
                if (S.items[i].visible == false) {
                    width = 0;
                } else {
                    width = S.items[i].size;
                }
            } else {
                autoCount++;
            }
        }
        return (maxSize - width - (S.items.length - 1) * lineSize) / autoCount;
    };
    var rFlag = false;
    T.resize = function () {
        //console.log((new Date()) + "框架改变");
        rFlag = true;
        var xFlag = S.type == null || S.type == "x";
        var bodyWidth = A.width(), bodyHeight = A.height();
        var maxSize = (xFlag) ? bodyWidth : bodyHeight;
        var leftXY = 0;
        var autoSize = getAutoSize(maxSize);

        for (var i = 0; i < S.items.length; i++) {
            if (i > 0) {
                if (xFlag) {
                    lines[i].css({ height: bodyHeight + "px", width: lineSize + "px" });
                }
                else {
                    lines[i].css({ width: bodyWidth + "px", height: lineSize + "px" });
                }
                leftXY += lineSize;
            }
            var size = typeof (S.items[i].size) == "number" ? S.items[i].size : autoSize;
            if (xFlag) {
                //if (T.items[i].controls) console.log("T.items[i].controls:" + T.items[i].controls.length + T.items[i].css);
                T.items[i].css({ height: bodyHeight + "px", width: size + "px" });
            } else {
                T.items[i].css({ width: bodyWidth + "px", height: size + "px" });
            }
            leftXY += size;
        }
        rFlag = false;
    };
    A.resize(function () {
        if (rFlag) return; T.resize();
    });
    T.resize();
    $M.BaseClass.apply(T, [S]);
    T.css = function (style) {
        A.css(style);
        T.resize();
    };
    if (S.style) T.css(S.style);
};
$M.Control["Tab"] = function (BoxID, S) {
    var ID = "Button_" + $M.Index + "_";
    var T = this;
    T.items = [];
    T.controls = [];
    T.selectedItem = null;
    var A = $("<div class=\"tabbable\" ></div>").appendTo(BoxID);
    var B = $("<ul class=\"nav nav-tabs\"></ul>").appendTo(A);
    var C = $("<div class=\"tab-content\"></div>").appendTo(A);
    if (S.alignment == 1) A.addClass("tabs-left");
    var item = function (S2) {
        var T2 = this;
        T.items[T.items.length] = T2;
        T.controls[T.controls.length] = T2;
        var titlehtml = "<li><a href='#'>";
        if (S2.ico) titlehtml += "<i class=\"fa " + S2.ico + "\"></i>";
        titlehtml += "<span class=caption >" + S2.text + "</span>";
        if (S2.closeButton) titlehtml += "<i class=\"close fa fa-times-circle\"></i>";
        titlehtml += "</a>";
        titlehtml += "</li>"
        var title = $(titlehtml).appendTo(B);
        var closeButton = title.find(".close");
        var caption = title.find(".caption");
        var content = $("<div class=\"tab-pane\"></div>").appendTo(C);
        if (S2.html) content.html(S2.html);
        if (S2["class"]) content.addClass(S2["class"]);

        T2.focus = function () {
            if (T.selectedItem) T.selectedItem.blur();
            title.addClass("active");
            content.addClass("active");
            T.selectedItem = T2;
            if (S.onSelectedIndexChanged) S.onSelectedIndexChanged(T, {});
            T2.resize();
        };
        T2.blur = function () {
            title.removeClass("active");
            content.removeClass("active");
        };
        T2.remove = function () {
            title.remove();
            content.remove();
            T.items = T.items.del(T.items.indexOf(T2));
            T.items[T.items.length - 1].focus();
            T.controls = T.controls.del(T2);
            T2 = null;
        };
        T2.resize = function () {
            //content.height(C.height());
            var w = content.width(), h = C.height();
            if (T2.controls) {
                var countHeight = 0;
                var dockC = [];
                for (var i = 0; i < T2.controls.length; i++) {
                    //console.warn(T2.controls[i]);
                    //console.warn(T2.controls[i].attr);
                    if (T2.controls[i].attr) {
                        if (T2.controls[i].attr("dock") == 2) {
                            dockC[dockC.length] = T2.controls[i];
                            //T2.controls[i].css({ width: w + "px", height: h + "px" });
                        } else {
                            //console.warn(T2.controls[i]);
                            if (T2.controls[i].height) countHeight += T2.controls[i].height();
                        }
                    }
                }
                //console.warn((h - countHeight) + "px");
                if (dockC.length > 0) {
                    console.warn(dockC[0].name);
                    //if (dockC[0].marginHeight) {
                    //    alert(1);
                    //}
                    var marginHeight = dockC[0].attr("marginHeight") ? dockC[0].attr("marginHeight") : 0;
                    dockC[0].css({ width: w + "px", height: (h - countHeight - marginHeight) + "px" });
                }
            }
        };
        T2.container = content;
        $M.BaseClass.apply(T2, [S2]);
        title.click(T2.focus);
        closeButton.click(function () {
            if (S2.onClose) {
                S2.onClose();
            }
            T2.remove();
        });
        this.attr = function (a, b) {
            if (b != null) S2[a] = b;
            if (a == "text") caption.html(b);
            return (S2[a]);
        };
    };
    T.addItem = function (S2) {
        return (new item(S2));
    };
    if (S.items) {
        for (var i = 0; i < S.items.length; i++) {
            T.addItem(S.items[i]);
        }
        T.items[0].focus();
    }
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    T.css = function (style) {
        if (style) {
            A.css(style);
            if (style && style.height) {
                //console.log("tab");
                if (S.alignment == 1) {
                    C.outerHeight(A.height());
                } else {
                    C.outerHeight(A.height() - B.height());
                }
                T.selectedItem.resize();
            }
        }
    };
    if (S.dock != null && S.dock == 2) {
        T.css({ width: BoxID.width(), height: BoxID.height() });
    }
    if (S.style) T.css(S.style);
};
$M.Control["Panel"] = function (BoxID, S) {
    var T = this;
    var html = "<div class=\"panel\" tabindex=1>";
    var phead = null, pbody = null;
    if (S.text) {
        html += "<div class=\"panel-heading\"><h3 class=\"panel-title\">";
        if (S.ico) html += "<i class=\"fa " + S.ico + "\"></i> ";
        html += S.text;
        html += "<span class=\"panel-options\"></span>";
        html += "</h3></div>";
        html += "<div class=\"panel-body\">";
        html += "</div>";

    }
    html += "</div>";
    var A = $(html).appendTo(BoxID);
    if (S.text == null) pbody = A;
    else {
        phead = A.find(".panel-heading");
        pbody = A.find(".panel-body");
    }
    if (S.color != null) A.addClass("panel-" + $M.Control.Constant.colorCss[S.color]);
    T.container = pbody;
    $M.BaseClass.apply(T, [S]);

    if (S["class"]) T.addClass(S["class"]);
    $(document).on("keydown", function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {

        }
    });
    if (S.url) pbody.load(S.url);
};
$M.Control["PanelGroup"] = function (BoxID, S) {
    var T = this;
    var A = $("<div class=\"row grid ui-sortable\"></div>").appendTo(BoxID);
    T.items = [];
    var boxItem = [];
    var blankBox = null;
    function exchangePos(elem1, elem2) {
        var parent = elem1.parent();
        var next = elem1.next(), next2 = elem2.next();
        if (next.length == 0) {
            parent.append(elem2);
        } else {
            next.before(elem2);
        }
        if (next2.length == 0) {
            parent.append(elem1);
        } else {
            next2.before(elem1);
        }
    }
    var move = function (_obj, _box, x, y) {
        var obj = null, box = null;
        var x1 = x + _box.width(), y1 = y + _box.height();
        for (var i = 0; i < boxItem.length; i++) {
            var xy = boxItem[i].position();
            w = boxItem[i].width(); h = boxItem[i].height();
            var f = w / 10, f2 = h / 10;
            //alert([f,f2])
            var pix1 = x > (xy.left + f) && y > (xy.top + f2) && x < (xy.left + w - f) && y < (xy.top + h - f2);
            var pix2 = x1 > (xy.left + f) && y1 > (xy.top + f2) && x1 < (xy.left + w - f) && y1 < (xy.top + h - f2);
            //var pix3 = x > (xy.left + f) && y1 > (xy.top + f2) && x < (xy.left + w - f) && y1 < (xy.top + h - f2);
            //var pix4 = x1 > (xy.left + f) && y > (xy.top + f2) && x1 < (xy.left + w - f) && y < (xy.top + h - f2);
            if (boxItem[i] != _box && (pix1 || pix2)) {
                box = boxItem[i];
                obj = T.items[i];
                i = boxItem.length;
            }
        }
        if (obj) {
            exchangePos(blankBox, box);
            swap(_obj, obj);
        }
    };
    var swap = function (a, b) {
        var _index = a.index;
        var temp = T.items[a.index];
        T.items[a.index] = T.items[b.index];
        T.items[b.index] = temp;
        temp = boxItem[a.index];
        boxItem[a.index] = boxItem[b.index];
        boxItem[b.index] = temp;
        a.index = b.index;
        b.index = _index;
    };
    T.addItem = function (S2) {
        if (S2.size == null) S2.size = 4;
        var box = $("<div class=\"col-md-" + S2.size + "\" ></div>").appendTo(A);
        S2.xtype = "Panel";
        var obj = box.addControl(S2);
        obj.index = boxItem.length;
        boxItem[obj.index] = box;
        T.items[obj.index] = obj;
        box.moveBox({
            onStart: function () { blankBox = $("<div class=\"col-md-2\"></div>").insertAfter(box); blankBox.css({ width: box.outerWidth() + "px", height: box.outerHeight() + "px" }); },
            onEnd: function () {
                blankBox.before(box);
                box.attr("style", "");
                blankBox.remove();
                //                var temp = [boxItem[tempIndex], T.items[tempIndex]];
                //                var newIndex = -1;
                //                for (var i1 = 0; i1 < T.items.length; i1++) {
                //                    if (T.items[i1] == obj) newIndex = i1;
                //                }

                //                if (newIndex > -1) {
                //                    boxItem[tempIndex] = boxItem[newIndex];
                //                    T.items[tempIndex] = T.items[newIndex];
                //                    boxItem[newIndex] = temp[0];
                //                    T.items[newIndex] = temp[1];
                //                }
                if (S.onChange) S.onChange(T, null);
            },
            onMove: function (sendder, e) {
                move(obj, box, e.x, e.y);
            }
        });
        return obj;
    };
    if (S.items) {
        for (var i = 0; i < S.items.length; i++) {
            T.addItem(S.items[i]);
        }
    }

    T.container = A;
    $M.BaseClass.apply(T, [S]);
};
$M.Control["SlidingBar"]=function(BoxID,S)
	{
		var B=BoxID.addDom("div");B.className="M5_SlidingBar";
		var T=this;
		T.items=new Array();
		T.openItem=null;
		this.add=function(S2){
			var T2=this;
			T2.buttons=new Array();
			T.items[T.items.length]=this;
			var Bar=B.addDom("div");Bar.className="Bar";Bar.unselectable="on";
			if(S.ico!=null){var Ico=Bar.addDom("div");Ico.className="Ico "+S.ico;}
			var Title=Bar.addDom("span");//Title.className="Title";
			Title.unselectable="on";
			var ButtonBox=Bar.addDom("div");
			ButtonBox.setFloat("right");
			if(S2.buttons!=null){
			    for(var i=0;i<S2.buttons.length;i++)
			    {
			        S2.buttons[i].xtype="Button"; S2.buttons[i].className="LabelButton";
			        S2.buttons[i].disabled="true";
			        T2.buttons[T2.buttons.length]=ButtonBox.addControl(S2.buttons[i]);
			    }
			}
			if(S2.titleMenu!=null)
		    {
			    Bar.addEvent("onmouseup",function(){
			        if($M.event.button()==2)S2.titleMenu.show($M.event.x()-2,$M.event.y()-2);
			    });
		    }
			//var Button=ButtonBox.addControl({xtype:"Button",ico:"addClass",onclick:function(){},className:"LabelButton"});
			var Box=B.addDom("div");Box.className="Box";Box.style.display="none";
			var h=1;
			if(S2.caption!=null)Title.innerHTML=S2.caption;
			T2.setCaption=function(text)
			{
				Title.innerHTML=text;
			};
			T2.buttonsDisabled=function(tag){
			    for(var i=0;i<T2.buttons.length;i++){
			        T2.buttons[i].disabled(tag);
			    }
			};
			T2.remove=function(){
			    var n=-1;
			    for(var i=0;i<T.items.length;i++){
			        if(T.items[i]==T2)n=i;
			    }
			    T.items=T.items.del(n);
			    Bar.remove();
			    Box.remove();
			    T.items[0].open();
			};
			T2.open=function()
			{
				if(T.openItem!=null){T.openItem.buttonsDisabled(true);T.openItem.close();}
				Box.style.display="";
				Box.style.height=B.offsetHeight-Bar.offsetHeight*T.items.length+"px";
				T.openItem=T2;
				T2.buttonsDisabled(false);
				if(S.onopen)S.onopen(T2);
			};
			T2.resize=function()
			{
			    var height=Bar.offsetHeight*T.items.length;
			    if(height>B.offsetHeight)height=0;
			    else{height=B.offsetHeight-height;}
				Box.style.height=height+"px";
			};
			T2.close=function()
			{
				Box.style.display="none";
				T.openItem=null;
			};
			T2.addControl=function(Set){return(Box.addControl(Set));};
			Bar.addEvent("onclick",T2.open);
			return(this);
		};
		this.resize=function(){
		    if(T.openItem!=null)T.openItem.resize();
		};
		this.setAttribute=function(a,b){S[a]=b;};
		this.setSize=function(w,h)
		{
			B.style.height=h+"px";
			if(T.openItem!=null)T.openItem.resize();
		};
		if(S.items!=null){
		for(var n=0;n<S.items.length;n++){
			var s=new this.add(S.items[n]);
		}
		}
};
$M.Control["Window"] = function (BoxID, S) {
    var objID = "Window_" + $.Index + "_";
    var T = this;
    var html = "<div class=\"modal-content M5_Window\" tabindex=\"-1\" style=\"display:none;z-index:" + ($M.zIndex + 1) + "\">";
    html += "<div class=\"modal-header\">";
    html += "<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-hidden=\"true\">×</button>";
    html += "<h4 class=\"modal-title\" >";
    if (S.ico) html += "<i class=\"fa " + S.ico + "\"></i> ";
    html += S.text;
    html += "</h4>";
    html += "</div>";
    html += "</div>";
    T.footer = [];
    var modal_body = null, modal_footer = null, closeButton = null;
    var A = $(html).appendTo(BoxID);
    if (S.style) A.css(S.style);
    var header = A.find(".modal-header");
    if (S.text == null) header.hide();
    closeButton = A.find(".modal-header .close");
    T.form = A.addControl({
        url: S.url,
        command: S.command,
        xtype: "Form",
        onBeginSubmit: S.onBeginSubmit,
        onSubmit: S.onSubmit
    });
    T.dialogResult = $M.dialogResult.cancel;
    var modal_body = T.form.append("<div class=\"modal-body\"></div>");

    //var modal_body = $("<div class=\"modal-body\"></div>").appendTo(form);
    if (S.footer) {
        modal_footer = T.form.append("<div class=\"modal-footer\"></div>");
        //modal_footer = $("<div class=\"modal-footer\"></div>").appendTo(form);
        for (var i = 0; i < S.footer.length; i++) {
            T.footer[T.footer.length] = modal_footer.addControl(S.footer[i]);
        }
    }
    T.remove = function () {
        var r = true;
        if (S.onClose) r = S.onClose(T,null);
        if (r == null || r) {
            if (S.isModal) {
                $M.lock(A, false);
            }
            A.remove();
            if (A[0] == $M.focusElement || A.has($M.focusElement).length) $M.focusElement = null;
        }
    };
    if (closeButton) {
        closeButton.click(function () { T.remove(); });
    }
    var keydown = function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {
            if (e.which == 27 && S.isModal && S.text != null) {
                T.remove();
            }
            if (S.onKeyDown) S.onKeyDown(T, e);
        }
    };

    $(document).on("keydown", keydown);
    T.container = modal_body;
    $M.BaseClass.apply(T, [S]);
    T.show = function () {
        if (S.isModal) $M.lock(A, true, T, true);
        A.show();
        $M.focusElement = A[0];
        if (S.style == null || (S.style != null && S.style.left == null && S.style.top == null)) {
            A.css({ left: ($(document.body).width() - A.width()) / 2 + "px", top: ($(document.body).height() - A.height()) / 2 + "px" });
        }
    };
    T.append = function (str) {
        return $(str).appendTo(modal_body);
    };
    T.addControl = function (S2) {
        var list = modal_body.addControl(S2);
        if (T.form != null) {
            if (T.form.controls == null) T.form.controls = [];
            if ($.isArray(list)) {
                for (var i = 0; i < list.length; i++) {
                    T.form.controls[T.form.controls.length] = list[i];
                }
            } else {
                T.form.controls[T.form.controls.length] = list;
            }
        }
        return list;
    };
};
$M.Control["Windows"] = function () {
    var T = this;
    var zIndex = 1000;
    T.items = new Array();
    var addItem = function () {

    };
};