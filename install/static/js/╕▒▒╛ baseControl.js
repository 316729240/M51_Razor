﻿var $M = {};
$M.Index = 0;
$M.zIndex = 1000;
$M.focusElement = null;
$M.Control= {
        Constant: {
            colorCss: ["default", "primary", "success", "info", "warning", "danger", "link"],
            color: {
                "default": 0,
                "primary": 1,//主要
                "success": 2,//正确
                "info": 3,//信息
                "warning": 4,//错误
                "danger": 5,//危险
                "link": 6
            },
            dockStyle: { "none": 0, "bottom": 1, "fill": 2, "left": 3, "right": 4, "top": 5 },
            sizeCss: ["xs", "sm", "", "lg"],
            size: { "xs": 0, "sm": 1, "default": 2, "lg": 3 }
        }
    };
    $M.ajax = function (url, data, back) {
        var type = data ? "POST" : "GET";
        $.ajax({
            url: url,
            type: type,
            success: back,
            data: data,
            dataType: "json",
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                alert("发生错误");
                //alert(XMLHttpRequest.status);
                //alert(XMLHttpRequest.readyState);
                //alert(textStatus);
            }
        });
    };
    $M.Method = {
        Array: function () {
            this.indexOf = function () {
                var C = this.length;
                for (var i = 0; i < C; i++) {
                    if (this[i] == arguments[0]) return i;
                };
                return -1;
            };
            this.del = function (n) {
                if (typeof (n) != "number") {
                    var index = this.indexOf(n);
                    if (index > -1) return this.slice(0, index).concat(this.slice(index + 1, this.length));
                } else {
                    if (n < 0)
                        return this;
                    else
                        return this.slice(0, n).concat(this.slice(n + 1, this.length));
                }
            };
        }
    };
$M.BaseClass = function (S) {
    this.attr = function (a, b) {
        if (b != null) S[a] = b;
        return (S[a]);
    };
    this.addControl = function (S2) {
        var obj = null;
        if (S2 == null) return;
        if (this.Container) {
            if (S2 && S2.length) {
                for (var i = 0; i < S2.length; i++) {
                    var line = null;
                    if (S2[i].length) {
                        line = $("<div class=\"formSep\"></div>").appendTo(this.Container);
                        line = $("<div class=\"row\"></div>").appendTo(line);
                        for (var i1 = 0; i1 < S2[i].length; i1++) {
                            var S3 = S2[i][i1];
                            S3.width = (S3.width == null ? "12" : S3.width);
                            var item = $("<div class=\"col-md-" + S3.width + "\"><label>" + S3.labelText + ((S3.vtype && S3.vtype.required) ? "<span class=\"f_req\">*</span>" : "") + "</label></div>").appendTo(line);
                            var obj = item.addControl(S3);

                            if (this.controls == null) this.controls = [];
                            this.controls[this.controls.length] = obj;
                        }
                    } else {
                        line = $("<div class=\"form-group\"></div>").appendTo(this.Container);
                        var S3 = S2[i];
                        S3.width = (S3.width == null ? "12" : S3.width);
                        var item = $("<label>" + S3.labelText + ((S3.vtype && S3.vtype.required) ? "<span class=\"f_req\">*</span>" : "") + "</label>").appendTo(line);
                        var obj = line.addControl(S3);
                        if (this.controls == null) this.controls = [];
                        this.controls[this.controls.length] = obj;
                    }
                }
            } else {
                obj = this.Container.addControl(S2);
                //obj.parent = this;
                if (this.controls == null) this.controls = [];
                this.controls[this.controls.length] = obj;
            }
        }
        return obj;
    };
    this.append = function (dom) {
        if (this.Container) {
            return this.Container.append(dom);
        };
        return null;
    };
};
$(document).on("focusin.M4.public", function (e) { $M.focusElement = e.target; });
$M.Method.Array.apply(Array.prototype);
$.fn.extend({
    addControl: function (S) {
        if (!S) return;
        $M.Index++;
        var obj = new $M.Control[S.xtype](this, S);
        try {
            return (obj);
        } finally {
            obj = null;
        }
    }, moveBox: function (S) {
        $(this).mousedown(function (e) {
            var _position = $(this).position();
            $(this).css({ "z-index": 1000, position: "absolute", top: _position.top, left: _position.left, width: $(this).outerWidth() + "px", height: $(this).outerHeight() + "px" });
            if (S.start) S.start();
            var o = {};
            o.iTop = _position.top - e.pageY;
            o.iLeft = _position.left - e.pageX;
            $this = $(this);
            $(document).bind("mousemove", function (e) {
                var iLeft = e.pageX + o.iLeft;
                var iTop = e.pageY + o.iTop;
                $this.css({
                    'left': iLeft + "px",
                    'top': iTop + "px"
                });
                if (S.move) S.move(iLeft, iTop);
            });
            $(document).bind("mouseup", function () {
                $(document).unbind("mousemove");
                $(document).unbind("mouseup");
                if (S.end) S.end();
            });

        });
    }
});

$M.Control["inputBox"] = function (BoxID, S) {
    var BZ = false;
    var T = this;
    var tipColor = "#C0C0C0";
    var defaultColor = "";
    var A = BoxID.addDom("div");
    A.className = "M4_inputBox " + S.cssClass;
    if (S.width != null) A.style.width = S.width;
    var B = A.addDom("input");
    B.className = "TextBox";
    B.style.width = (A.offsetWidth - 21) + "px";
    B.name = S.name;
    if (S.value != null) B.value = S.value;
    var C = A.addDom("div");
    C.className = "Button";
    C.unselectable = "on";
    this.text = "";
    this.value = "";
    var tempValue = "";
    this.clearBoth = function () {
        A.style.clear = "both"
    };
    var D = null;
    B.addEvent("onfocus", function () {
        B.style.color = defaultColor;
        if (B.value == S.tip) {
            B.value = ""
        };
        B.className = "TextBox_focus";
        C.className = "Button_focus";
        BZ = true;
        if (S.onfocus != null) S.onfocus()
    });
    B.addEvent("onblur", function () {
        if (S.tip != null && B.value == "") {
            B.value = S.tip;
            B.style.color = tipColor
        };
        B.className = "TextBox";
        C.className = "Button";
        BZ = false;
        if (S.onblur != null) S.onblur()
    });
    B.addEvent("onkeyup", function () {
        if (tempValue != B.value) {
            tempValue = B.value;
            if (S.oninput != null) S.oninput(T)
        };
        if ($.event.keyCode() == 13 && S.onconfirm != null) S.onconfirm()
    });
    C.addEvent("onmousedown", function () {
        C.className = "Button_Down";
        if (S.onconfirm != null) S.onconfirm()
    });
    C.addEvent("onmouseup", function () {
        C.className = "Button_Up";
        B.className = "TextBox_focus";
        B.focus();
        BZ = true
    });
    this.remove = function () {
        B.remove();
        C.remove();
        A.remove();
        A = null;
        B = null;
        C = null
    };
    this.focus = function () {
        B.focus()
    };
    this.setValue = function (v) {
        B.style.color = defaultColor;
        B.value = v
    };
    this.val = function () {
        if (B.value == S.tip) return ("");
        else {
            return (B.value)
        }
    };
    this.setAttribute = function (a, b) {
        S[a] = b
    };
    this.setSize = function (w, h) {
        A.setSize(w, h);
        B.setSize(A.offsetWidth - 21, h)
    };
    C.className = "Button_Up";
    B.className = "TextBox_focus";
    B.focus()
};
$M.Control["dialogBox"] = function (BoxID, S) {
    var DateOpen = false,
        Cfocus = false;
    var T = this;
    var A = BoxID.addDom("div");
    A.className = "M4_DialogBox";
    if (S.width != null) A.style.width = S.width + "px";
    var B = null;
    if ((S.height != null && S.height > 22) || S.autoHeight) {
            B = A.addDom("textarea");
            B.style.overflow = "hidden"
        } else {
            B = A.addDom("input")
        }
    B.className = "TextBox";
    B.style.width = (A.offsetWidth - 21) + "px";
    B.name = S.name;
    if (S.value != null) B.value = S.value;
    var C = A.addDom("div");
    C.className = "Button";
    C.unselectable = "on";
    C.setSize(null, B.offsetHeight - 1);
    if (S.position != null) A.style.position = S.position;
    if (S.top) A.style.top = S.top + "px";
    if (S.left) A.style.left = S.left + "px";
    this.text = "";
    this.value = "";
    this.clearBoth = function () {
            A.style.clear = "both"
        };
    var D = null;
    A.addEvent("onmouseover", function () {
            Cfocus = true
        });
    A.addEvent("onmouseout", function () {
            if (!DateOpen) Cfocus = false
        });
    B.addEvent("onfocus", function () {
            B.className = "TextBox_focus";
            C.className = "Button_focus";
            if (S.onfocus != null) S.onfocus()
        });
    B.addEvent("onblur", function () {
            if (!Cfocus) {
                B.className = "TextBox";
                C.className = "Button";
                if (S.onblur != null) S.onblur()
            }
        });
    C.addEvent("onmousedown", function () {
            C.className = "Button_Down"
        });
    C.addEvent("onmouseup", function () {
            C.className = "Button_Up";
            B.className = "TextBox_focus";
            B.focus();
            if (S.onopendialog != null) {
                DateOpen = true;
                S.onopendialog(function (value) {
                    DateOpen = false;
                    B.focus();
                    Cfocus = false;
                    if (value) T.setValue(value)
                }, T)
            }
        });
    this.setSize = function (w, h) {
            A.setSize(w, h);
            B.setSize(A.offsetWidth - 21, h)
        };
    this.remove = function () {
            B.remove();
            C.remove();
            A.remove();
            A = null;
            B = null;
            C = null
        };
    this.focus = function () {
            B.focus()
        };
    this.setValue = function (v) {
            B.value = v
        };
    this.val = function () {
            return (B.value)
        };
    this.setAttribute = function (a, b) {
            S[a] = b
        };
    this.focus()
};
$M.Control["uploadFileBox"] = function (BoxID, S, CID) {
    var DateOpen = false;
    var T = this,
        ieTag = $.Browse.isIE(),
        A;
    T.err = false;
    T.allPathTag = false;
    T.name = S.name;
    if (CID != null) A = CID;
    else {
            A = BoxID.addDom("div")
        }
    A.className = "M4_UploadfileBox";
    if (S.width != null) A.style.width = S.width + "px";
    var B = A.addDom("input");
    B.className = "TextBox";
    B.style.width = (A.offsetWidth - 21) + "px";
    B.name = S.name;
    if (S.value != null) B.value = S.value;
    var C = A.addDom("div");
    C.className = "Button";
    C.unselectable = "on";
    C.setSize(null, B.offsetHeight);
    var ifrname = "uploadFileBox_ifr_" + $.Index,
        ifr = null;
    if (ieTag && !$.Browse.isIE9()) {
            ifr = A.addDom("iframe");
            try {
                ifr.src = managedir + "uploadifr.html"
            } catch (x) {
                ifr.src = "/manage/uploadifr.html"
            }
            ifr.style.display = "none";
            C.addEvent("onclick", function () {
                var file = ifr.add();
                file.click();
                if (file.value != "") {
                    if (checkFile(file.value, S.file_types)) ifr.submit()
                }
                B.focus()
            })
        } else {
            ifr = $D.createElement("iframe");
            ifr.name = ifrname;
            ifr.style.display = 'none';
            document.body.appendChild(ifr);
            ifr = $.Function.applyObj(ifr)
        }
    ifr.uploadEnd = function (V) {
            if (S.allPath) B.value = V.uploaddir + V.path;
            else {
                B.value = V.path
            }
            T.setfocus();
            DateOpen = false;
            if (S.onchange != null) S.onchange()
        };
    if (!ieTag || $.Browse.isIE9()) {
            var C1 = C.addDom("div");
            C1.cssText("width:15px;height:20px;overflow:hidden;filter:alpha(opacity=0);  filter: progid:DXImageTransform.Microsoft.Alpha(opacity=0);  -moz-opacity:0; opacity:0;");
            var file = null,
                form = null;
            form = C1.addDom("form");
            form.target = ifrname;
            form.action = "upload.aspx";
            form.method = "POST";
            form.enctype = "multipart/form-data";
            var input1 = form.addDom("input");
            input1.name = "type";
            input1.value = "2";
            input1.style.display = "none";
            var input2 = form.addDom("input");
            input2.name = "tag";
            input2.value = "2";
            input2.style.display = "none";
            file = form.addDom("input");
            file.type = "file";
            file.name = "FileData";
            file.cssText("width:15px;height:20px;");
            file.addEvent("onclick", function () {
                    B.focus()
                });
            file.addEvent("onchange", function () {
                    if (checkFile(file.value, S.file_types)) form.submit();
                    B.focus()
                })
        }
    if (S.position != null) A.style.position = S.position;
    if (S.top != null) A.style.top = S.top + "px";
    if (S.left != null) A.style.left = S.left + "px";
    this.text = "";
    this.value = "";
    this.clearBoth = function () {
            A.style.clear = "both"
        };
    var D = null;
    B.addEvent("onchange", function () {
            if (S.onchange != null) S.onchange()
        });
    B.addEvent("onfocus", function () {
            B.className = "TextBox_focus";
            C.className = "Button_focus";
            if (S.onfocus != null) S.onfocus()
        });
    B.addEvent("onblur", function () {
            if (!DateOpen) {
                B.className = "TextBox";
                C.className = "Button";
                if (S.onblur != null) S.onblur()
            }
        });
    A.addEvent("onmouseover", function () {
            DateOpen = true
        });
    A.addEvent("onmouseout", function () {
            DateOpen = false
        });
    var checkFile = function (str, kzm) {
            if (kzm == "*.*") return (true);
            var kzm2 = kzm;
            kzm = (kzm.replace(/;/g, "|"));
            kzm = (kzm.replace(/,/g, "|"));
            kzm = (kzm.replace(/\*/g, ""));
            var strRegex = "(" + kzm + ")$";
            var re = new RegExp(strRegex, "i");
            if (re.test(str)) {
                return (true)
            } else {
                alert("选择文件有误，只允许上传(" + kzm2 + ")");
                return (false)
            }
        };
    this.setSize = function (w, h) {
            A.setSize(w, h);
            B.setSize(A.offsetWidth - 21, h)
        };
    T.check = function () {
            var V = B.value.verification({
                maxLen: S.maxLen,
                minLen: S.minLen,
                minValue: S.minValue,
                maxValue: S.maxValue
            });
            if (S.repeat != null) {
                if ($(S.repeat).value != T.val()) {
                    V.R = false
                }
            }
            if (!V.R) {
                B.className = "TextBox_focus";
                T.err = true;
                if (S.errMsg != null && S.errMsg != "") {
                    T.errMsg = S.errMsg
                } else {
                    T.errMsg = V.M
                }
                ShowMsg(T.errMsg)
            } else {
                B.className = "TextBox_focus";
                T.err = false;
                T.errMsg = ""
            }
        };
    var Msg = null;
    var ShowMsg = function (text) {
            if (Msg == null) {
                Msg = $B.addControl({
                    xtype: 'Tip',
                    HTML: text,
                    className: 'M4_TipErrBox'
                });
                Msg.setXY(B.getXY().left + 30, B.getXY().top + 30)
            }
            var hide = function () {
                if (Msg != null) {
                    Msg.remove();
                    Msg = null
                }
            };
            setTimeout(hide, 1500)
        };
    var HideMsg = function () {};
    B.addEvent("onkeyup", function () {
            T.check()
        });
    this.remove = function () {
            ifr.remove;
            B.remove();
            C.remove();
            A.remove()
        };
    this.setfocus = function () {
            B.focus()
        };
    this.setValue = function (v) {
            B.value = v;
            if (S.onchange != null) S.onchange()
        };
    this.val = function () {
            return (B.value)
        };
    this.setAttribute = function (a, b) {
            S[a] = b
        };
    T.setfocus();
    DateOpen = false
};
$M.Control["PathBar"] = function (BoxID, S) {
    var T = this;
    T.items = new Array();
    var Box = BoxID.addDom("div");
    if (S.ico != null) {
        Box.className = "M4_PathBar " + S.ico
    } else {
        Box.className = "M4_PathBar"
    }
    var pathBox = Box.addDom("div");
    var textBox = Box.addDom("input");
    textBox.style.display = "none";
    Box.addEvent("onmousedown", function () {
        if ($.event.target().tagName != "DIV" || pathBox.style.display == "none") return;
        pathBox.style.display = "none";
        textBox.style.display = "";
        textBox.value = T.val();
        Box.className += " PathEdit";
        setTimeout(setf,100);
    });
    var setf=function(){
      textBox.focused=true;
      textBox.select();
    };
    Box.addEvent("onkeydown", function () {
        if ($.event.keyCode() == 13) {
            if (S.ongopath != null) S.ongopath(textBox.value)
        }
    });
    textBox.addEvent("onblur", function () {
        pathBox.style.display = "";
        textBox.style.display = "none";
        Box.className = Box.className.replace(" PathEdit", "")
    });
    T.setSize = function (w, h) {
        Box.setSize(w, h);
        textBox.setSize(w - 10, null)
    };
    T.add = function (value) {
        T.clear();
        T.items[T.items.length] = value;
        T.reload()
    };
    T.clear = function () {
        var count = pathBox.childNodes.length;
        for (var i = 0; i < count; i++) pathBox.childNodes[0].remove()
    };
    T.val = function () {
        return (T.items[T.items.length - 1].value)
    };
    T.go = function (i) {
        T.items.length = i + 1;
        T.reload();
        if (S.onchange != null) S.onchange(T.items[i])
    };
    T.reload = function () {
        T.clear();
        for (var i = 0; i < T.items.length; i++) {
            if (i > 0) pathBox.addDom("span").innerHTML = "->";
            var text = pathBox.addDom("a");
            text.href = "#";
            eval('text.addEvent("onclick",function(){T.go(' + i + ');})');
            text.innerHTML = T.items[i].text
        }
    }
};
$M.Control["PageBar"] = function (BoxID, S) {
    var T = this;
    this.recordCount = 0;
    this.pageSize = 0;
    this.pageno = 0;
    var ToolBar = BoxID.addControl({
        xtype: "ToolBar"
    });
    var F = ToolBar.add({
        ico: "page-first",
        onclick: function () {
            T.goPage(1)
        }
    });
    var P = ToolBar.add({
        ico: "page-prev",
        onclick: function () {
            T.goPage(T.pageno - 1)
        }
    });
    var TrackBar1 = ToolBar.add({
        xtype: "TrackBar",
        min: 1,
        max: 10,
        width: 200,
        onchange: function (value) {
            if (T.goPage) T.goPage(value)
        }
    });
    var N = ToolBar.add({
        ico: "page-next",
        onclick: function () {
            T.goPage(T.pageno + 1)
        }
    });
    var L = ToolBar.add({
        ico: "page-last",
        onclick: function () {
            T.goPage(parseInt((T.recordCount - 1) / T.pageSize) + 1)
        }
    });
    ToolBar.add({
        caption: "|"
    });
    var R = ToolBar.add({
        ico: "refresh",
        onclick: function () {
            T.goPage(T.pageno)
        }
    });
    ToolBar.add({
        caption: "|"
    });
    var Label2 = ToolBar.add({
        xtype: "Label",
        caption: ""
    });
    ToolBar.add({
        caption: "|"
    });
    var Label = ToolBar.add({
        xtype: "Label",
        caption: ""
    });
    this.height = ToolBar.height;
    this.setAttribute = function (A, B) {
        S[A] = B
    };
    this.goPage = function (pageno) {
        if (S.onchange != null && this.reload(pageno)) S.onchange(pageno)
    };
    this.reload = function (pageno) {
        pageno = parseInt(pageno);
        if (pageno == null || pageno == "") pageno = 1;
        var pageCount = parseInt((this.recordCount - 1) / this.pageSize + 1);
        TrackBar1.setAttribute("max", pageCount);
        if (pageno >= 1 && pageno <= pageCount) {
            Label2.setCaption("第" + pageno + "页");
            Label.setCaption("共" + this.recordCount + "条记录 分" + pageCount + "页显示 每页显示" + this.pageSize + "条记录");
            if (pageno == 1) {
                F.disabled(true);
                P.disabled(true)
            } else {
                F.disabled(false);
                P.disabled(false)
            }
            if (pageno == pageCount) {
                L.disabled(true);
                N.disabled(true)
            } else {
                L.disabled(false);
                N.disabled(false)
            }
            TrackBar1.setValue(pageno);
            this.pageno = pageno;
            return (true)
        }
    }
};
$M.Control["Panel"] = function (BoxID, S) {
    var T = this;
    var A = BoxID.addDom("div");
    A.className = "Panel";
    var Title_Div = A.addDom("div");
    Title_Div.className = "Title_L";
    Title_Div.addEvent("onmousedown", function () {
        if (S.onmovestart != null) S.onmovestart($.event.x(), $.event.y())
    });
    Title_Div.addEvent("onmousemove", function () {
        if (S.onmove != null) S.onmove($.event.x(), $.event.y())
    });
    Title_Div.addEvent("onmouseup", function () {
        if (S.onmoveend != null) S.onmoveend($.event.x(), $.event.y())
    });
    this.getXY = function () {
        return (A.getXY())
    };
    this.setCapture = function () {
        Title_Div.setCapture()
    };
    this.releaseCapture = function () {
        Title_Div.releaseCapture()
    };
    this.setXY = function (x, y) {
        if (x != null) A.style.left = x + "px";
        if (y != null) A.style.top = y + "px"
    };
    this.insertDom = function (o) {
        return (A.insertDom(o))
    };
    this.getSize = function () {
        return ({
            width: A.offsetWidth,
            height: A.offsetHeight
        })
    };
    this.replaceObj = function (obj) {
        if (obj != null) {
            var M = obj.parentNode.insertBefore(A, obj);
            T.setPosition("");
            obj.remove()
        }
    };
    this.remove = function () {
        A.remove();
        A = null
    };
    this.addDom = function (D) {
        return (Content.addDom(D))
    };
    this.addControl = function (D) {
        return (Content.addControl(D))
    };
    this.setPosition = function (p) {
        A.style.position = p
    };
    var B = Title_Div.addDom("div");
    B.className = "Title_R";
    B = B.addDom("div");
    B.className = "Title_C";
    var Caption = B.addDom("div");
    Caption.className = "Caption";
    var ButtonDiv = B.addDom("div");
    ButtonDiv.className = "Button";
    var maxButton, setButton, closeButton;
    if (S.setButton != null) {
        setButton = ButtonDiv.addDom("a");
        setButton.className = "Set";
        setButton.href = "#";
        setButton.addEvent("onmousedown", function () {
            if (S.onclose != null) S.onclose();
            T.remove();
            return (false)
        })
    }
    if (S.closeButton != null) {
        closeButton = ButtonDiv.addDom("a");
        closeButton.className = "Close";
        closeButton.href = "#";
        closeButton.addEvent("onmousedown", function () {
            T.remove();
            if (S.onclose != null) S.onclose();
            return (false)
        })
    }
    B = A.addDom("div");
    B.className = "Content_L";
    B = B.addDom("div");
    B.className = "Content_R";
    var Content = B.addDom("div");
    Content.className = "Content_C";
    var Foot_Div = A.addDom("div");
    Foot_Div.className = "Foot_L";
    B = Foot_Div.addDom("div");
    B.className = "Foot_R";
    B = B.addDom("div");
    B.className = "Foot_C";
    this.setCaption = function (text) {
        Caption.innerHTML = text
    };
    this.getCaption = function () {
        return (Caption.innerHTML)
    };
    if (S.caption != null) this.setCaption(S.caption);
    this.setSize = function (w, h) {
        if (w != null) A.style.width = w + "px";
        if (h != null) Content.style.height = (h - Title_Div.offsetHeight - Foot_Div.offsetHeight) + "px"
    };
    if (S.width != null) A.style.width = S.width;
    if (S.height != null) this.setSize(null, S.height);
    if (S.onload != null) S.onload(T)
};

$M.Control["Label"] = function (BoxID, S, CID) {
    var B = null;
    if (CID != null) B = CID;
    else {
        B = BoxID.addDom("label");
    }
    B.className = "Label";
    B.style.height = "auto";
    this.name = S.name;
    if (S.htmlFor != null) B.htmlFor = S.htmlFor;
    if (S.width != null) {
        if (S.width == "100%") B.style.width = "100%";
        else {
            B.setSize(S.width, null)
        }
    }
    this.setCaption = function (text) {
        B.innerHTML = text
    };
    this.val = function () {
        return(B.innerHTML);
    };
    this.setValue = function (text) {
        B.innerHTML = text
    };
    this.setFontSize = function (size) {
        B.style.fontSize = size
    };
    this.setSize = function (w, h) {
        B.setSize(w, h)
    };
    this.setLineHeight = function (height) {
        B.style.lineHeight = height + "px"
    };
    if (S.caption != null) this.setCaption(S.caption);
    if (S.value != null) this.setValue(S.value);
    if (S.size != null && S.width == null) this.setSize(S.size);
    if (S.clearBoth) B.style.clear = "both";
    if (S.lineHeight) this.setLineHeight(S.lineHeight);
    switch (S.align) {
    case "left":
        B.style.textAlign = "left";
        break;
    case "right":
        B.style.textAlign = "right";
        break
    }
    this.remove = function () {
        B.remove();
        B = null
    };
    this.offsetWidth = B.offsetWidth
};
$M.Control["ColorBox"] = function (BoxID, S) {
    var OpenTag = false;
    var BZ = false;
    var T = this;
    var A = BoxID.addDom("div");
    A.className = "M4_ColorBox";
    A.unselectable = "on";
    var S1 = $B.addControl({
        xtype: 'Shadow',
        width: 0,
        height: 0,
        top: 0,
        left: 0
    });
    S1.setZIndex(2000);
    var bodyEvent = function () {
        if (OpenTag && !BZ) {
            T.hide()
        }
    };
    this.show = function (x, y) {
        $D.addEvent("onmousedown", bodyEvent);
        OpenTag = true;
        A.show();
        S1.show();
        A.style.top = y + "px";
        A.style.left = x + "px";
        var WZ = A.getXY();
        S1.setXY(WZ.left, WZ.top)
    };
    this.setAttribute = function (a, b) {
        S[a] = b
    };
    this.setZIndex = function (I) {
        if ($.Browse.isIE()) A.style.zIndex = I
    };
    this.hide = function () {
        $D.removeEvent("onmousedown", bodyEvent);
        OpenTag = false;
        A.hide();
        S1.hide();
        if (S.onclose != null) S.onclose()
    };
    var value = "";
    this.setZIndex(2001);
    var colorlist = new Array(40);
    colorlist[0] = "#000000";
    colorlist[1] = "#993300";
    colorlist[2] = "#333300";
    colorlist[3] = "#003300";
    colorlist[4] = "#003366";
    colorlist[5] = "#000080";
    colorlist[6] = "#333399";
    colorlist[7] = "#333333";
    colorlist[8] = "#800000";
    colorlist[9] = "#FF6600";
    colorlist[10] = "#808000";
    colorlist[11] = "#008000";
    colorlist[12] = "#008080";
    colorlist[13] = "#0000FF";
    colorlist[14] = "#666699";
    colorlist[15] = "#808080";
    colorlist[16] = "#FF0000";
    colorlist[17] = "#FF9900";
    colorlist[18] = "#99CC00";
    colorlist[19] = "#339966";
    colorlist[20] = "#33CCCC";
    colorlist[21] = "#3366FF";
    colorlist[22] = "#800080";
    colorlist[23] = "#999999";
    colorlist[24] = "#FF00FF";
    colorlist[25] = "#FFCC00";
    colorlist[26] = "#FFFF00";
    colorlist[27] = "#00FF00";
    colorlist[28] = "#00FFFF";
    colorlist[29] = "#00CCFF";
    colorlist[30] = "#993366";
    colorlist[31] = "#CCCCCC";
    colorlist[32] = "#FF99CC";
    colorlist[33] = "#FFCC99";
    colorlist[34] = "#FFFF99";
    colorlist[35] = "#CCFFCC";
    colorlist[36] = "#CCFFFF";
    colorlist[37] = "#99CCFF";
    colorlist[38] = "#CC99FF";
    colorlist[39] = "#FFFFFF";
    ocbody = "";
    var i = 0;
    for (var y = 0; y < 5; y++) {
        for (var x = 0; x < 8; x++) {
            var box = A.addDom("div");
            box.className = "color";
            box.cssText("background-color:" + colorlist[i] + ";");
            eval('box.addEvent("onmousedown",function(){T.hide();if(S.onselect!=null)S.onselect("' + colorlist[i] + '");});');
            i++
        }
    }
    S1.setHeight(A.offsetHeight);
    this.hide()
};
$M.Control["ListBox"] = function (BoxID, S) {
    var OpenTag = 0;
    var BZ = false;
    var T = this;
    A = BoxID.addDom("div");
    A.className = "M4_ListBox";
    A.unselectable = "on";
    if (S.width) A.style.width = S.width + "px";
    if (S.height) A.style.height = S.height + "px";
    A.style.top = S.top + "px";
    A.style.left = S.left + "px";
    this.setZIndex = function (I) {
        A.style.zIndex = I
    };
    if (S.ZIndex != null) T.setZIndex(S.ZIndex);
    A.addEvent("onmouseover", function () {
        BZ = true;
        if (S.onMouseover != null) S.onMouseover()
    });
    A.addEvent("onmouseout", function () {
        BZ = false;
        if (S.onMouseout != null) S.onMouseout()
    });
    this.width = A.offsetWidth;
    this.height = A.offsetHeight;
    var Dclick = function () {
        if (!BZ) {
            if (OpenTag > 0) {
                if (S.onBlur != null) S.onBlur()
            }
            OpenTag++
        }
    };
    $D.addEvent("onclick", Dclick);
    this.setXY = function (X, Y) {
        if (X != null) A.style.left = X + "px";
        if (Y != null) A.style.top = Y + "px"
    };
    this.addItem = function (text, value) {
        var B = A.addDom("div");
        B.unselectable = "on";
        if (S.type == "color") {
            B.innerHTML = "<span style='float:left;width:12px;height:12px;background-color: " + value + ";overflow:hidden;margin-right:5px;'></span><span>" + text + "</span>"
        } else {
            B.innerHTML = text
        }
        B.value = value;
        B.addEvent("onmouseover", function () {
            B.className = "SelectItem"
        });
        B.addEvent("onmouseout", function () {
            B.className = ""
        });
        B.addEvent("onclick", function () {
            if (S.onSelectItem != null) S.onSelectItem(text, value)
        })
    };
    this.removeItem = function (n) {
        A.childNodes[n].remove()
    };
    this.removeAll = function () {
        var c = A.childNodes.length;
        for (var i = 0; i < c; i++) {
            A.childNodes[0].remove()
        }
    };
    if (S.items != null) {
        for (var i = 0; i < S.items.length; i++) {
            if (S.items[i].text != null) this.addItem(S.items[i].text, S.items[i].value);
            else {
                this.addItem(S.items[i].caption, S.items[i].value)
            }
        }
    }
    this.remove = function () {
        A.remove();
        $D.removeEvent('onclick', Dclick)
    }
};
$M.Control["Radio"] = function (BoxID, S) {
    var T = this;
    T.disabledTag = false;
    if (S.disabled) T.disabledTag = S.disabled;
    var A = BoxID.addDom("div");
    if (S.clearBoth) A.style.clear = "both";
    if (S.checked) {
        A.className = "Radio Checked"
    } else {
        A.className = "Radio"
    }
    this.checked = S.checked;
    this.value = S.value;
    this.SelectItem = function (Tag) {
        if (!Tag) {
            A.className = "Radio";
            T.checked = false;
        } else {
            A.className = "Radio Checked";
            T.checked = true;
        }
    };
    A.addEvent("onclick", function () {
        if (T.disabledTag) return;
        T.SelectItem(true);
        if (S.onclick != null) S.onclick()
    })
};
$M.Control["RadioGroup"] = function (BoxID, S, CID) {
    var T = this;
    var B = null;
    T.disabledTag = false;
    if (S.disabled) T.disabledTag = S.disabled;
    if (CID != null) B = CID;
    else {
        B = BoxID.addDom("div")
    };
    B.className = "M4_Radio";
    T.name = S.name;
    var I = B.addDom("input");
    I.style.display = "none";
    I.name = S.name;
    var G = new Array();
    var SelectItem = function (id) {
        for (var i = 0; i < G.length; i++) {
            G[i].SelectItem(false)
        }
        id.SelectItem(true);
        I.value = id.value;
        if (S.onchange != null) S.onchange(T)
    };
    T.val = function () {
        return (I.value)
    };
    T.setSize = function (w, h) {
        B.setSize(w, h)
    };
    T.setAttribute = function (a, b) {
        S[a] = b
    };
    T.addItem = function (M) {
        var C = B.addControl({
            xtype: "Radio",
            checked: M.checked,
            value: M.value==null?M.caption:M.value,
            clearBoth: M.clearBoth,
            disabled: T.disabledTag,
            onclick: function () {
                SelectItem(C)
            }
        });
        G[G.length] = C;
        var L = B.addDom("div");
        L.className = "Text";
        L.innerHTML = M.caption;
        if (!T.disabledTag) {
            L.tabIndex = 1;
            L.addEvent("onclick", function () {
                SelectItem(C)
            });
            L.addEvent("onkeyup", function (e) {
                if ($.event.keyCode() == 13) SelectItem(C)
            })
        } else {
            L.className = L.className + " Disabled"
        }
        if (M.checked) SelectItem(C)
    };
    T.setValue = function (V) {
        for (var i = 0; i < G.length; i++) {
            if (G[i].value == V) {
                SelectItem(G[i])
            }
        }
    };
    if (S.items != null) {
        for (var i = 0; i < S.items.length; i++) {
            this.addItem(S.items[i])
        }
    }
    this.disabled = function (tag) {
        T.disabledTag = tag;
        if (tag) {
            B.style.filter = "Alpha(Opacity=30)";
        } else {
            B.style.filter = "Alpha(Opacity=100)"
        }
    };
    if (S.value != null) T.setValue(S.value);
    this.disabled(S.disabled)
};
$M.Control["CheckBox"] = function (BoxID, S, CID) {
    var T = this;
    var A = null;
    if (CID != null) A = CID;
    else {
        A = BoxID.addDom("div")
    }
    T.checked = false;
    T.name = S.name;
    if (S.marginLeft != null) {
        A.cssText("margin-left:" + S.marginLeft + "px;")
    }
    if (S.checked) {
        A.className = "CheckBox_Checked";
        T.checked = true
    } else {
        A.className = "CheckBox"
    }
    this.checked = false;
    if (S.checked != null) this.checked = S.checked;
    T.value = S.value;
    T.click = function () {
        if (T.checked) {
            T.checked = false;
            T.setValue(false)
        } else {
            T.checked = true;
            T.setValue(true)
        }
        if (S.onclick != null) {
            S.onclick(T)
        }
    };
    T.setSize = function (w, h) {};
    T.setValue = function (value) {
        if (value == "0") value = false;
        if (value == "1") value = true;
        if (value) {
            A.className = "CheckBox_Checked";
            T.checked = value
        } else {
            A.className = "CheckBox";
            T.checked = value
        }
    };
    T.remove = function () {
        A.remove();
        A = null
    };
    T.setAttribute = function (a, b) {
        S[a] = b
    };
    A.addEvent("onclick", this.click)
};
$M.Control["CheckBoxGroup"] = function (BoxID, S, CID) {
    var T = this;
    var B = null;
    if (CID != null) B = CID;
    else {
        B = BoxID.addDom("div")
    }
    B.className = "CheckBoxGroup";
    var I = B.addDom("input");
    I.style.display = "none";
    I.name = S.name;
    var G = new Array();
    T.name = S.name;
    this.setAttribute = function (a, b) {
        S[a] = b
    };
    T.val = function () {
        var Value = "";
        for (var i = 0; i < G.length; i++) {
            if (G[i].checked) {
                if (Value != "") Value = Value + ",";
                Value = Value + G[i].value
            }
        }
        I.value = Value;
        return (Value)
    };
    T.setValue = function (v) {
        var v = v.split(",");
        for (var i1 = 0; i1 < G.length; i1++) {
            G[i1].setValue(false)
        }
        for (var i = 0; i < v.length; i++) {
            if (v[i] != "") {
                for (i1 = 0; i1 < G.length; i1++) {
                    if (G[i1].value == v[i]) G[i1].setValue(true)
                }
            }
        }
    };
    T.setSize = function (w, h) {
        B.setSize(w, h)
    };
    T.addItem = function (M) {
        var A = B.addDom("div");
        if (M.align != "left" && G.length > 0) A.style.clear = "both";
        M["xtype"] = "CheckBox";
        M["onclick"] = function () {
            T.val();
            if (S.onclick != null) S.onclick()
        };
        var C = A.addControl(M);
        G[G.length] = C;
        var L = A.addDom("div");
        L.className = "Text";
        L.innerHTML = M.caption;
        L.tabIndex = 1;
        L.addEvent("onclick", C.click);
        L.addEvent("onkeyup", function (e) {
            if ($.event.keyCode() == 13) C.click()
        });
        this.val()
    };
    T.remove = function () {
        for (var i = 0; i < G.length; i++) {
            G[i].remove()
        }
        G = null;
        B.remove();
        B = null;
        I.remove();
        I = null
    };
    if (S.items != null) {
        for (var i = 0; i < S.items.length; i++) {
            this.addItem(S.items[i])
        }
    }
    if (S.value != null) T.setValue(S.value);
    T.val()
};
$M.Control["SelectBox"] = function (BoxID, S, CID) {
    var T = this;
    var A = null;
    if (CID != null) A = CID;
    else {
        A = $("<select class=\"form-control\" ></select>").appendTo(BoxID);
    }
    A.attr({ "name": S.name, "id": S.id, "disabled": S.disabled ? true : false });
    if (S.style) A.css(S.style);
    if (S.cssClass) A.addClass(S.cssClass);
    A.addClass("input-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
    $M.BaseClass.apply(T, [S]);
    S.items = S.items == null ? [] : S.items;
    var loadFlag = true;
    T.addItem = function (items) {
        if (typeof (items.length) != "undefined") {
            for (var i = 0; i < items.length; i++) {
                A.append("<option value=\"" + items[i].value + "\">" + items[i].text + "</option>");
                if (!loadFlag) S.items[length] = items[i];
            }
        } else {
            A.append("<option value=\"" + items.value + "\" >" + items.text + "</option>");
            if (!loadFlag) S.items[length] = items;
        }
    };
    T.val = function (value) {
        if (value)A.val(value);
        return(A.val());
    };
    T.addItem(S.items);
    loadFlag = false;
    T.val(S.value);
    A.change(function () { if (S.onChange) S.onChange(T); });
    return;
    T.disabledTag = false;
    if (S.disabled) T.disabledTag = true;
    var OpenTag = false;
    var A = null;
    var inputTag = S.inputTag;
    if (inputTag == null) inputTag = false;
    if (S.width == null) S.width = 200;

    A.className = "M4_Select";
    if (S.width != null) A.style.width = S.width + "px";
    if (S.ZIndex != null) A.style.zIndex = 10000;
    T.name = S.name;
    if (S.position != null) A.style.position = S.position;
    if (S.top != null) A.style.top = S.top + "px";
    if (S.left != null) A.style.left = S.left + "px";
    var B = A.addDom("input");
    B.className = "TextBox";
    B.name = S.name;
    B.style.width = (S.width - 22) + "px";
    B.style.height = "18px";
    var B2 = A.addDom("div");
    B2.className = "TextBox";
    B2.style.width = (S.width - 22) + "px";
    B2.unselectable = "on";
    B2.style.cursor = "default";
    if (!T.disabledTag) B2.tabIndex = "0";
    if (inputTag) B2.style.display = "none";
    else {
        B.style.display = "none"
    }
    var B1 = A.addDom("input");
    B1.style.display = "none";
    var C = A.addDom("div");
    C.className = "Button";
    C.unselectable = "on";
    if (T.disabledTag) {
        B.className = "TextBox_Disabled";
        B2.className = "TextBox_Disabled";
        C.className = "Button_Disabled"
    }
    this.getIndex = function (v) {
        if (S.items != null) {
            for (var i = 0; i < S.items.length; i++) {
                if ((S.items[i].value != null && S.items[i].value == v) || (S.items[i].value == null && S.items[i].caption == v)) {
                    return i;
                }
            }
        }
        return null;
    };
    this.removeValue = function (v) {
        var i = this.getIndex(v);
        if (i != null) {
            S.items = S.items.del(i);
        }
        this.setIndex(0);
    };
    this.setValue = function (v) {
        var i = this.getIndex(v);
        if (i != null) {
            var text = "";
            if (S.items[i].text != null) text = S.items[i].text;
            else {
                text = S.items[i].caption
            }
            B.value = text;
            B2.innerHTML = text;
            B1.value = (S.items[i].value == null) ? text : S.items[i].value;
            if (S.onchange != null) S.onchange(T);
        }
        //        if (S.items != null) {
        //            for (var i = 0; i < S.items.length; i++) {
        //                if ((S.items[i].value != null && S.items[i].value == v) || (S.items[i].value == null && S.items[i].caption == v)) {
        //                    var text = "";
        //                    if (S.items[i].text != null) text = S.items[i].text;
        //                    else {
        //                        text = S.items[i].caption
        //                    }
        //                    B.value = text;
        //                    B2.innerHTML = text;
        //                    B1.value = (S.items[i].value == null) ? text : S.items[i].value;
        //                    if (S.onchange != null) S.onchange(T);
        //                    return
        //                }
        //            }
        //        }
    };
    this.setIndex = function (index) {
        T.setValue(S.items[index].value)
    };
    if (S.value != null) T.setValue(S.value);
    this.text = "";
    this.value = "";
    this.clearBoth = function () {
        A.style.clear = "both"
    };
    this.setSize = function (w, h) {
        A.setSize(w, h);
        B.setSize(w - 21, null);
        B2.setSize(w - 21, null)
    };
    B.addEvent("onfocus", function () {
        if (T.disabledTag) return;
        B.className = "TextBox_focus";
        B2.className = "TextBox_focus";
        C.className = "Button_focus";
        if (S.onfocus != null) S.onfocus()
    });
    B.addEvent("onblur", function () {
        B.className = "TextBox";
        B2.className = "TextBox";
        C.className = "Button";
        if (S.onblur != null && !OpenTag) S.onblur()
    });
    var openBox = function () {
        if (T.disabledTag) return;
        if (!OpenTag) {
            C.className = "Button_Up";
            B.className = "TextBox_focus";
            B2.className = "TextBox_focus";
            if (inputTag) B.focus();
            else {
                B2.focus()
            }
            var S1 = $B.addControl({
                xtype: 'Shadow',
                width: 0,
                height: 0,
                top: 0,
                left: 0
            });
            var WZ = A.getXY();
            var obj = B2;
            if (inputTag) obj = B;
            var L_height = null;
            var bottomY = $B.scrollTop + $B.offsetHeight,
                rightX = $B.scrollLeft + $B.offsetWidth;
            if (S.items && S.items.length > 10) L_height = 200;
            var L = $B.addControl({
                xtype: "ListBox",
                items: S.items,
                ZIndex: 200000,
                width: (obj.offsetWidth + C.offsetWidth - 2),
                top: (obj.offsetHeight + WZ.top),
                left: WZ.left,
                type: S.type,
                height: L_height,
                onBlur: function () {
                    OpenTag = false;
                    S1.remove();
                    L.remove();
                    if (S.onblur != null) S.onblur()
                },
                onSelectItem: function (text, value) {
                    B.value = text;
                    this.text = text;
                    this.value = value;
                    B1.value = (value == null) ? text : value;
                    B2.innerHTML = text;
                    OpenTag = false;
                    S1.remove();
                    L.remove();
                    if (inputTag) B.focus();
                    else {
                        B2.focus()
                    };
                    if (S.onchange != null) S.onchange(T)
                }
            });
            var L_Left = WZ.left,
                L_Top = (obj.offsetHeight + WZ.top);
            if ((WZ.top + L.height) > bottomY) L_Top = WZ.top - L.height;
            L.setXY(L_Left, L_Top);
            S1.setZIndex(2000);
            S1.setWidth(L.width);
            S1.setHeight(L.height);
            S1.setXY(L_Left, L_Top);
            OpenTag = true
        }
    };
    var inputfocus = function () {
        if (!T.disabledTag) {
            B.className = "TextBox_focus";
            B2.className = "TextBox_focus";
            C.className = "Button_focus";
            B2.focus();
            if (S.onfocus != null) S.onfocus()
        }
    };
    B2.addEvent("onfocus", inputfocus);
    B2.addEvent("onblur", function () {
        if (T.disabledTag) return;
        B.className = "TextBox";
        B2.className = "TextBox";
        C.className = "Button";
        if (S.onblur != null && !OpenTag) S.onblur()
    });
    B2.addEvent("onmouseup", openBox);
    C.addEvent("onmousedown", function () {
        if (T.disabledTag) return;
        C.className = "Button_Down"
    });
    C.addEvent("onmouseup", openBox);
    this.addItem = function (text, value) {
        if (S.items == null) S.items = new Array();
        S.items[S.items.length] = {
            text: text,
            value: value
        }
    };
    this.disabled = function (tag) {
        T.disabledTag = tag;
        if (tag) {
            B.className = "TextBox_Disabled";
            B2.className = "TextBox_Disabled";
            C.className = "Button_Disabled"
        } else {
            B.className = "TextBox";
            B2.className = "TextBox";
            C.className = "Button"
        }
    };
    this.clear = function () {
        if (S.items != null) S.items.length = 0
    };
    this.remove = function () {
        C.remove();
        B.remove();
        A.remove()
    };
    this.setfocus = function () {
        if (!T.disabledTag) {
            if (inputTag) B.focus();
            else {
                inputfocus()
            }
        }
    };
    this.focus = function () {
        if (!T.disabledTag) {
            if (inputTag) B.focus();
            else {
                inputfocus()
            }
        }
    };
    this.val = function () {
        return (B1.value)
    };
    this.setAttribute = function (a, b) {
        S[a] = b
    };
    this.focus()
};
$M.Control["ButtonCheck"] = function (BoxID, S) {
    var objID = "ButtonCheck_" + $.Index + "_";
    var buttonID = objID + "0",
        leftID = objID + "1",
        rightID = objID + "2";
    var T = this,
        style = "";
    T.value = S.value;
    var disabledTag = false,
        menuOpen = false;
    var A = BoxID.addDom("div");
    A.className = "M4_Button";
    A.unselectable = "on";
    if (S.width) style += "width:" + S.width + "px;";
    if (S.float) style += "float:" + S.float + ";";
    if (S.marginRight) style += "margin-right:" + S.marginRight + "px";
    A.cssText(style);
    var buttonCss = "";
    if (S.ico) buttonCss = " class='Ico " + S.ico + "'";
    var tip = "";
    if (S.tip != null) tip = S.tip;
    var html = "<div class='left' id='" + leftID + "' title='" + tip + "'><div class='right'><div class='content button'><button unselectable=on id='" + buttonID + "' " + buttonCss + "></button></div></div></div>";
    A.innerHTML = html;
    leftID = $(leftID);
    buttonID = $(buttonID);
    if (S.onclick != null) leftID.addEvent("onclick", function () {
            if (!disabledTag) S.onclick()
        });
    if (S.caption) buttonID.innerHTML = S.caption;
    var down = function () {
            T.checked = true;
            leftID.className = "left_down"
        };
    var up = function () {
            T.checked = false;
            leftID.className = "left"
        };
    leftID.addEvent("onmousedown", down);
    this.setValue = function (tag) {
            if (tag) {
                down()
            } else {
                up()
            }
        };
    this.setCaption = function (text) {
            buttonID.innerHTML = text
        };
    this.val = function () {
            return (T.value)
        };
    if (S.checked != null) T.setValue(S.checked)
};
$M.Control["ButtonCheckGroup"] = function (BoxID, S) {
    var T = this;
    T.items = new Array();
    T.selectItem = function (id) {
        for (var i = 0; i < T.items.length; i++) {
            T.items[i].setValue(false)
        }
        id.setValue(true);
        T.value = id.value;
        if (S.onclick != null) S.onclick(T);
        if (S.onchange != null) S.onchange(T)
    };
    this.addItem = function (M) {
        var C = BoxID.addControl({
            xtype: "ButtonCheck",
            checked: M.checked,
            caption: M.caption,
            value: M.value,
            ico: M.ico,
            tip: M.tip,
            onclick: function () {
                T.selectItem(C)
            }
        });
        T.items[T.items.length] = C;
        if (M.checked) T.selectItem(C)
    };
    if (S.items != null) {
        for (var i = 0; i < S.items.length; i++) {
            this.addItem(S.items[i])
        }
    };
    this.val = function () {
        return (T.value)
    };
    this.setValue = function (V) {
        var c1 = T.items.length;
        for (var i = 0; i < c1; i++) {
            if (T.items[i].val() == V) {
                T.selectItem(T.items[i]);
                i = c1
            }
        }
    };
    this.setAttribute = function (a, b) {
        S[a] = b
    }
};
$M.Control["Button"] = function (BoxID, S, CID) {
    var T = this;
    var A = null;
    var group = null, menuButton = null;
    T.val = function (text) {
        var html = (S.ico != null) ? "<i class=\"" + S.ico + "\"></i>" : "";
        if (S.text != null) html += S.text;
        A.html(html);
    };
    if (CID != null) A = CID;
    else {
        if (S.menu != null) { group = $("<div class=\"btn-group\"/>").appendTo(BoxID); BoxID = group; }
        A = $("<button class=\"btn\"/>").appendTo(BoxID);
        T.val(S.text);
        A.attr({ "name": S.name, "id": S.id, "disabled": S.disabled ? true : false });
        if (S.style) A.css(S.style);
        if (S.cssClass) A.addClass(S.cssClass);
        A.addClass("btn-" + $M.Control.Constant.colorCss[S.color == null ? 0 : S.color]);
        A.addClass("btn-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
        if (group) {
            if (S.event != null && S.event.click != null) {
                menuButton = $("<button type=\"button\" class=\"btn\"><span class=\"caret\"></span></button>").appendTo(group);
                menuButton.addClass("btn-" + $M.Control.Constant.colorCss[S.color == null ? 0 : S.color]);
                menuButton.addClass("btn-" + $M.Control.Constant.sizeCss[S.size == null ? 1 : S.size]);
            } else {
                $("<span class=\"caret\"></span>").appendTo(A);
                menuButton = A;
            }
            menuButton.addClass("dropdown-toggle");
        }
        A.attr("type", S.type);
    }
    T.focus = function () {
        A.focus();
    };
    var objID = "Button_" + $.Index + "_";
    A.attr("id", objID);
    A.click(function () {
        var f = true;
        if (S.onClick) f = S.onClick();
        return f;
    });
    if (S.menu) {
        menuButton.on("click", function () {
            if (S.menu) {
                group.addClass("open");
                var xy = group.offset();
                S.menu.open(null, null, group);
            }
        });
        S.menu.on("close", function () {
            group.removeClass("open");
        });
    }
    $M.BaseClass.apply(T, [S]);
};
$M.Control["DomainUpDown"] = function (BoxID, S,CID) {
    var G = new Array();
    var Button = null;
    this.name = S.name;
    if (CID != null) A = CID;
    else {
            A = BoxID.addDom("div")
        };
    A.className = "M4_DomainUpDown";
    if (S.width != null) A.style.width = S.width + "px";
    if (S.position != null) A.style.position = S.position;
    if (S.top != null) A.style.top = S.top;
    if (S.left != null) A.style.left = S.left;
    var B = A.addDom("input");
    B.className = "TextBox";
    B.style.width = (A.offsetWidth - 26) + "px";
    B.name = S.name;
    if (S.value != null) B.value = S.value;
    var C = A.addDom("div");
    C.className = "Button";
    var B1 = C.addDom("div");
    B1.className = "Up";
    B1.unselectable = "on";
    var B2 = C.addDom("div");
    B2.className = "Down";
    B2.unselectable = "on";
    this.setSize = function (w, h) {
        A.setSize(w, h);
        B.setSize(A.offsetWidth - 26, h)
    };
    var AddData = function (T) {
        if (G.length > 0) {
            var index = G.indexOf(B.value);
            if (T == "Up") index++;
            if (T == "Down") index--;
            if (index > G.length - 1) index = 0;
            if (index < 0) index = G.length - 1;
            B.value = G[index]
        } else {
            if (S.type == null || S.type == "number") {
                if (S.step == null) S.step = 1;
                if (parseInt(B.value) + "" == "NaN") B.value = S.min;
                if (T == "Up") B.value = parseInt(B.value) + S.step;
                if (T == "Down") B.value = parseInt(B.value) - S.step;
                if (parseInt(B.value) > S.max) B.value = S.min;
                if (parseInt(B.value) < S.min) B.value = S.max
            }
        }
    };
    var ButtonDown = function () {
        if (Button != null) {
            if (S.onchange != null) S.onchange();
            AddData(Button);
            setTimeout(ButtonDown, 100)
        }
    };
    var BDown = function () {
        if (Button == "Up") B1.className = "Up";
        if (Button == "Down") B2.className = "Down";
        Button = null;
    };
    var Up = function () {
        B.focus();
        B.select();
        B1.className = "Up_Down";
        AddData("Up");
        Button = "Up";
        setTimeout(ButtonDown, 500)
    };
    var Down = function () {
        B.focus();
        B.select();
        B2.className = "Down_Down";
        AddData("Down");
        Button = "Down";
        setTimeout(ButtonDown, 500)
    };
    B.addEvent("onfocus", function () {
        B.className = "TextBox_focus";
        C.className = "Button_focus";
        if (S.onfocus != null) S.onfocus()
    });
    B.addEvent("onblur", function () {
        B.className = "TextBox";
        C.className = "Button";
        B1.className = "Up";
        B2.className = "Down";
        if (S.onblur != null) S.onblur()
    });
    B.addEvent("onchange", function () {
        if (S.onchange != null) S.onchange()
    });
    B1.addEvent("onmousedown", Up);
    B2.addEvent("onmousedown", Down);
    B.addEvent("onkeydown", function () {
        if ($.event.keyCode() == 38) {
            Up()
        };
        if ($.event.keyCode() == 40) {
            Down()
        }
    });
    $D.addEvent("onmouseup", BDown);
    $D.addEvent("onkeyup", BDown);
    if (S.items != null) G = S.items;
    this.addItem = function (value) {
        G[G.length] = value
    };
    this.setAttribute = function (a, b) {
        S[a] = b
    };
    this.val = function () {
        return (B.value)
    };
    this.setValue = function (value) {
        B.value = value
    };
    this.remove = function () {
        A.remove();
        G = null;
        $D.removeEvent('onmouseup', BDown);
        $D.removeEvent('onkeyup', BDown)
    };
    if (S.value != null)this.setValue(S.value);
    B.focus()
};
$M.Control["TextBox"] = function (BoxID, S, CID) {
    var T = this;
    var B = null;
    if (S.password != null) {
        B = $("<input type=password class=form-control />").appendTo(BoxID);
    } else {
        if ((S.height != null && S.height > 22) || S.autoHeight) {
            B = $("<textarea class=form-control />").appendTo(BoxID);
        } else {
            B = $("<input class=form-control />").appendTo(BoxID);
        }
    }
    B.attr({ "name": S.name, "id": S.id, "disabled": S.disabled ? true : false, "placeholder": S.placeholder });
    if (S.style) B.css(S.style);
    if (S.cssClass) B.addClass(S.cssClass);
    if (S.vtype) B.attr(S.vtype);
    /*
    defaultColor = B.style.color;
    if (S.tip != null) {
    B.value = S.tip;
    B.style.color = tipColor
    }
    this.setSize = function (w, h) {
    T.DivBox.setSize(w, h);
    B.setSize(T.DivBox.offsetWidth - 4, h)
    };
    this.disabled = function (tag) {
    B.disabled = tag
    };

    B.addEvent("onfocus", function () {
    B.style.color = defaultColor;
    if (B.value == S.tip) {
    B.value = ""
    }
    verification();
    if (!T.err) {
    B.className = "TextBox_focus"
    }
    });
    B.addEvent("onblur", function () {
    if (S.tip != null && B.value == "") {
    B.value = S.tip;
    B.style.color = tipColor
    }
    if (!T.err) {
    verification();
    B.className = "TextBox"
    }
    if (S.onblur != null) S.onblur()
    });
    var verification = function () {
    var V = B.value.verification({
    maxLen: S.maxLen,
    minLen: S.minLen,
    type: S.vtype,
    minValue: S.minValue,
    maxValue: S.maxValue
    });
    if (S.repeat != null) {
    if ($(S.repeat).value != T.val()) {
    V.R = false
    }
    }
    if (!V.R) {
    B.className = "TextBox_Err";
    T.err = true;
    if (S.errMsg != null && S.errMsg != "") {
    T.errMsg = S.errMsg
    } else {
    T.errMsg = V.M
    }
    ShowMsg(T.errMsg)
    } else {
    B.className = "TextBox_focus";
    T.err = false;
    T.errMsg = ""
    }
    };
    T.check = function () {
    verification()
    };
    var Msg = null;
    var ShowMsg = function (text) {
    if (Msg == null) {
    Msg = $B.addControl({
    xtype: 'Tip',
    HTML: text,
    className: 'M4_TipErrBox'
    });
    Msg.setXY(B.getXY().left + 30, B.getXY().top + 30)
    }
    var hide = function () {
    if (Msg != null) {
    Msg.remove();
    Msg = null
    }
    };
    setTimeout(hide, 1500)
    };
    var HideMsg = function () {};
    B.addEvent("onmouseover", function () {
    if (T.err) {
    ShowMsg(T.errMsg)
    }
    });
    this.setfocus = function () {
    B.focus()
    };
    this.setValue = function (v) {
    if (v != "") {
    B.style.color = defaultColor;
    B.value = v
    } else {
    B.style.color = tipColor;
    if (S.tip != null) {
    B.value = S.tip
    } else {
    B.value = ""
    }
    }
    };
    B.addEvent("onchange", function () {
    if (S.onchange != null) S.onchange();
    verification()
    });
    B.addEvent("onkeyup", function () {
    if (S.onkeyup != null) S.onkeyup();
    verification()
    });
    */
    T.val = function (value) {
        if (value == null) {
            if (B.val() == S.tip) {
                return ("")
            } else {
                return (B.val())
            }
        } else {
            B.val(value);
        }
    };
    T.remove = function () {
        B.remove();
        HideMsg();
        B = null;
        this.DivBox.remove();
        D = null;
        this.DivBox = null;
        verification = null
    };
    $M.BaseClass.apply(T, [S]);
};
$M.Control["Shadow"] = function (BoxID, Setting) {
    var B = BoxID.addDom("div");
    if (Setting.width != null) B.style.width = Setting.width + "px";
    if (Setting.height != null) B.style.height = Setting.height + "px";
    if (Setting.left != null) B.style.left = Setting.left + "px";
    if (Setting.top != null) B.style.top = Setting.top + "px";
    B.className = "M4_Shadow";
    this.setWidth = function (W) {
        if ($.Browse.isIE()) {
            B.style.width = W + "px"
        } else {
            B.style.width = W - 5 + "px"
        }
    };
    this.setHeight = function (H) {
        if ($.Browse.isIE()) {
            B.style.height = H + "px"
        } else {
            B.style.height = H - 5 + "px"
        }
    };
    this.setZIndex = function (I) {
        B.style.zIndex = I
    };
    this.setLeft = function (L) {
        if ($.Browse.isIE()) {
            B.style.left = L + "px"
        } else {
            B.style.left = L + "px"
        }
    };
    this.setTop = function (T) {
        if ($.Browse.isIE()) {
            B.style.top = T + "px"
        } else {
            B.style.top = T + "px"
        }
    };
    this.setXY = function (l, t) {
        if ($.Browse.isIE()) {
            B.style.top = t - 5 + "px";
            B.style.left = l - 5 + "px"
        } else {
            B.style.top = t + "px";
            B.style.left = l + "px"
        }
    };
    this.hide = function () {
        B.hide()
    };
    this.show = function () {
        B.show()
    };
    this.remove = function () {
        B.remove();
        B = null
    }
};
$M.Control["Tip"] = function (BoxID, Setting) {
    var S = null;
    if ($.Browse.isIE()) {
        S = BoxID.addControl({
            xtype: 'Shadow',
            width: 0,
            height: 0,
            top: 0,
            left: 0
        })
    };
    var B = BoxID.addDom("div");
    B.style.zIndex = 10000;
    B.className = Setting.className;
    var A = B.addDom("div");
    A.className = "left";
    A = A.addDom("div");
    A.className = "right";
    A = A.addDom("div");
    A.className = "C";
    A = B.addDom("div");
    A.className = "left1";
    A = A.addDom("div");
    A.className = "right1";
    var M = A.addDom("div");
    M.className = "C1";
    A = B.addDom("div");
    A.className = "left2";
    A = A.addDom("div");
    A.className = "right2";
    A = A.addDom("div");
    A.className = "C2";
    B.style.width = "200px";
    if (Setting.width != null) B.style.width = Setting.width + "px";
    this.remove = function () {
        B.remove();
        if (S != null) {
            S.remove();
            S = null
        };
        A = null;
        B = null
    };
    this.show = function () {
        B.show();
        S.show()
    };
    this.setXY = function (X, Y) {
        B.style.left = X + "px";
        B.style.top = Y + "px";
        if ($.Browse.isIE()) {
            S.setWidth(B.offsetWidth - 5);
            S.setHeight(B.offsetHeight - 5);
            S.setTop(B.offsetTop);
            S.setLeft(B.offsetLeft)
        }
    };
    if (Setting.HTML != null) M.innerHTML = Setting.HTML;
};
$M.Control["Err"] = function (BoxID, Setting) {
    var B = BoxID.addDom("div");
    var T;
    B.className = "Err";
    var ShowMsg = function () {
        T = $B.addControl({
            xtype: 'Tip',
            HTML: Setting.text,
            className: 'M4_TipErrBox'
        });
        T.setXY(B.getXY().left + 30, B.getXY().top + 30)
    };
    var HideMsg = function () {
        setTimeout(T.remove, 500)
    };
    this.hide = function () {
        B.remove();
        B = null;
        T = null
    };
    B.addEvent("onmouseover", ShowMsg);
    B.addEvent("onmouseout", HideMsg)
};
$M.Control["DateWindow"] = function (BoxID, Setting) {
    var T = this;
    var TempY, TempM;
    var SelectMonth = null,
        SelectYear = null;
    var S1 = BoxID.addControl({
            xtype: "Shadow",
            top: Setting.top,
            left: Setting.left
        });
    S1.setZIndex(10000);
    var A = BoxID.addDom("div");
    A.className = "M4_DateWindow";
    A.unselectable = "on";
    A.style.zIndex = 10000;
    A.style.top = Setting.top + "px";
    A.style.left = Setting.left + "px";
    var B = A.addDom("div");
    B.className = "Left1";
    var SelectYear1 = null,
        SelectYear2 = null;
    B = A.addDom("div");
    B.className = "C1";
    var LB = B.addDom("a");
    LB.className = "Left";
    var C = B.addDom("span");
    C.className = "Title";
    var YearMonth = C.addDom("a");
    YearMonth.innerHTML = "";
    var S = C.addDom("span");
    S.innerHTML = "&nbsp;";
    var NB = B.addDom("a");
    NB.className = "Right";
    B = A.addDom("div");
    B.className = "Right1";
    var Tool = A.addDom("div");
    Tool.className = "Tool";
    Tool.innerHTML = "<span>日</span><span>一</span><span>二</span><span>三</span><span>四</span><span>五</span><span>六</span>";
    var Day = A.addDom("div");
    Day.className = "Day";
    var End = A.addDom("div");
    End.className = "End";
    var SelectDate = A.addDom("div");
    SelectDate.className = "SelectDate";
    var MonthList = SelectDate.addDom("div");
    MonthList.className = "MonthList";
    var YearList = SelectDate.addDom("div");
    YearList.className = "YearList";
    var Button = YearList.addDom("div");
    Button.className = "Button";
    var LeftButton = Button.addDom("a");
    LeftButton.className = "left";
    LeftButton.addEvent("onclick", function () {
            showYearList(-1)
        });
    var RightButton = Button.addDom("a");
    RightButton.className = "right";
    RightButton.addEvent("onclick", function () {
            showYearList(1)
        });
    var YearList2 = YearList.addDom("div");
    var End2 = SelectDate.addDom("div");
    End2.className = "End";
    var Today = End.addControl({
            xtype: "Button",
            width: 50,
            caption: "今日",
            align: "",
            onclick: function () {
                var RQ = new Date();
                T.year = RQ.getFullYear();
                T.month = RQ.getMonth() + 1;
                T.day = RQ.getDate();
                showDay(T.year, T.month)
            }
        });
    var Canecl = End2.addControl({
            xtype: "Button",
            width: 50,
            caption: "取消",
            align: "right",
            onclick: function () {
                HideSelectDate()
            }
        });
    var S = End2.addDom("div");
    S.style.width = "5px";
    S.style.styleFloat = "right";
    var OK = End2.addControl({
            xtype: "Button",
            width: 50,
            caption: "确定",
            align: "right",
            onclick: function () {
                HideSelectDate();
                showDay(TempY, TempM)
            }
        });
    SelectDate.style.display = "none";
    var showYearList = function (Tag) {
            ShowYear = function (Y1) {
                var B1 = YearList2.addDom("a");
                B1.innerHTML = Y1;
                if (Y1 == T.year) {
                    B1.className = "Select";
                    SelectYear = B1
                }
                B1.addEvent("onclick", function () {
                    SelectYear.className = "";
                    B1.className = "Select";
                    SelectYear = B1;
                    TempY = Y1
                });
            };
            var Y = T.year;
            if (Tag == -1) Y = SelectYear1;
            if (Tag == 1) Y = SelectYear2;
            var Count = YearList2.childNodes.length;
            for (var i = 0; i < Count; i++) {
                YearList2.childNodes[0].remove()
            }
            for (var i = Y - 4; i <= Y + 5; i++) {
                ShowYear(i)
            }
            SelectYear1 = Y - 10;
            SelectYear2 = Y + 10
        };
    var getMonthDay = function (year, month) {
            var tmp = new Date(year, month, 0);
            return tmp.getDate()
        };
    var getDayWeek = function (D) {
            var tmp = new Date(Date.parse(D));
            return tmp.getDay()
        };
    var nextMonth = function () {
            T.month++;
            if (T.month > 12) {
                T.month = 1;
                T.year++
            }
            showDay(T.year, T.month)
        };
    var lastMonth = function () {
            T.month--;
            if (T.month < 1) {
                T.month = 12;
                T.year--
            }
            showDay(T.year, T.month)
        };
    var showDay = function (year, month) {
            T.year = year;
            T.month = month;
            YearMonth.innerHTML = "" + year + "年" + month + "月";
            var LC = getMonthDay(year, month - 1);
            var SD = getDayWeek(year + "/" + month + "/1");
            var SC = getMonthDay(year, month);
            var i1 = 1;
            var Count = Day.childNodes.length;
            var AddDay = function (Y, M, D) {
                if (M == 0) {
                    Y--;
                    M = 12
                }
                var C = Day.addDom("a");
                C.innerHTML = D;
                if (M != T.month) C.className = "Invalid";
                if (T.year == Y && T.month == M && T.day == D) C.className = "Select";
                C.addEvent("onclick", function () {
                    T.year = Y;
                    T.month = M;
                    T.day = D;
                    if (Setting.onchange != null) Setting.onchange()
                })
            };
            for (var i = 0; i < Count; i++) {
                Day.childNodes[0].remove()
            }
            for (var i = LC - SD + 1; i <= LC; i++) {
                i1++;
                AddDay(year, month - 1, i)
            }
            for (var i = 1; i <= SC; i++) {
                AddDay(year, month, i);
                i1++
            }
            var i2 = 0;
            for (var i = i1; i1 <= 42; i1++) {
                i2++;
                AddDay(year, month + 1, i2)
            }
        };
    LB.addEvent("onclick", lastMonth);
    NB.addEvent("onclick", nextMonth);
    var RQ = null;
    if (Setting.value == null) {
            RQ = new Date()
        } else {
            RQ = new Date(Setting.value);
            if (isNaN(RQ.getFullYear())) RQ = new Date()
        }
    T.year = RQ.getFullYear();
    T.month = RQ.getMonth() + 1;
    T.day = RQ.getDate();
    var showTag = true;
    var ShowSelectDate = function () {
            showTag = true;
            var t = 0,
                d = 50;
            var U = function () {
                    t++;
                    var H = $.Tween.Elastic.easeOut(t, -168, 168, d);
                    SelectDate.style.top = H + "px";
                    if (t < d && showTag) setTimeout(U, 10);
                    else {
                        SelectDate.style.top = "0px"
                    }
                };
            TempM = T.month;
            TempY = T.year;
            var XY = A.getXY();
            SelectDate.style.display = "";
            SelectDate.style.top = "-168px";
            SelectYear = T.year;
            showMonthList();
            showYearList();
            U()
        };
    var HideSelectDate = function () {
            showTag = false;
            var t = 0,
                d = 40;
            var U = function () {
                    t++;
                    var H = $.Tween.Expo.easeOut(t, 0, 200, d);
                    SelectDate.style.top = -H + "px";
                    if (t < d && !showTag) setTimeout(U, 10);
                    else {
                        SelectDate.style.display = "none"
                    }
                };
            U()
        };
    var showMonthList = function () {
            var Month = function (M1) {
                var M = MonthList.addDom("a");
                M.innerHTML = M1 + "月";
                if (M1 == T.month) {
                    M.className = "Select";
                    SelectMonth = M
                }
                M.addEvent("onclick", function () {
                    SelectMonth.className = "";
                    M.className = "Select";
                    SelectMonth = M;
                    TempM = M1
                })
            };
            var Count = MonthList.childNodes.length;
            for (var i = 0; i < Count; i++) {
                MonthList.childNodes[0].remove()
            }
            for (var i = 1; i <= 12; i++) {
                Month(i)
            }
        };
    var BZ = false;
    A.addEvent("onmouseover", function () {
            BZ = true
        });
    A.addEvent("onmouseout", function () {
            BZ = false
        });
    var Dclick = function () {
            if (BZ) {
                if (Setting.onfocus != null) Setting.onfocus()
            } else {
                if (Setting.onBlur != null) Setting.onBlur()
            }
        };
    $D.addEvent("onclick", Dclick);
    YearMonth.addEvent("onclick", ShowSelectDate);
    showDay(T.year, T.month);
    S1.setWidth(A.offsetWidth);
    S1.setHeight(A.offsetHeight);
    S1.setXY(A.offsetLeft, A.offsetTop);
    if (Setting.onfocus != null) Setting.onfocus();
    this.remove = function () {
            A.remove();
            S1.remove();
            A = null;
            S1 = null;
            $D.removeEvent("onclick", Dclick)
        }
};
$M.Control["DateBox"] = function (BoxID, S, CID) {
    var BZ = false,
        DateOpen = false,
        T = this,
        A = null;
    T.err = false;
    T.name = S.name;
    if (S.format == null) S.format = "YYYY-MM-DD";
    if (CID != null) A = CID;
    else {
            A = BoxID.addDom("div")
        }
    A.className = "M4_DateBox";
    if (S.width != null) A.style.width = S.width + "px";
    var B = A.addDom("input");
    B.className = "TextBox";
    B.style.height = "20px";
    B.style.width = (A.offsetWidth - 21) + "px";
    B.name = S.name;
    if (S.ZIndex != null) A.style.zIndex = 10000;
    var C = A.addDom("div");
    C.className = "Button";
    C.unselectable = "on";
    if (S.position != null) A.style.position = S.position;
    if (S.top != null) A.style.top = S.top + "px";
    if (S.left != null) A.style.left = S.left + "px";
    this.text = "";
    this.value = "";
    this.clearBoth = function () {
            A.style.clear = "both"
        };
    var D = null;
    B.addEvent("onmouseover", function () {
            if (T.err) {
                ShowMsg(T.errMsg)
            }
        });
    B.addEvent("onfocus", function () {
            B.className = "TextBox_focus";
            C.className = "Button_focus";
            BZ = true;
            if (S.onfocus != null) S.onfocus()
        });
    B.addEvent("onblur", function () {
            B.className = "TextBox";
            C.className = "Button";
            BZ = false;
            if (S.onblur != null && !DateOpen) S.onblur()
        });
    C.addEvent("onmousedown", function () {
            C.className = "Button_Down"
        });
    C.addEvent("onmouseup", function () {
            C.className = "Button_Up";
            B.className = "TextBox_focus";
            B.focus();
            BZ = true;
            if (D == null) {
                var Value = T.val();
                if (Value == "") Value = new Date();
                else {
                    Value = Value.replace("年", "/");
                    Value = Value.replace("月", "/");
                    Value = Value.replace("日", "");
                    Value = Value.replace("-", "/")
                }
                var XY = A.getXY();
                DateOpen = true;
                D = $B.addControl({
                    xtype: "DateWindow",
                    value: Value,
                    top: (XY.top + A.offsetHeight),
                    left: XY.left,
                    onBlur: function () {
                        if (!BZ) {
                            D.remove();
                            D = null;
                            DateOpen = false;
                            if (S.onblur != null && !DateOpen) S.onblur()
                        }
                    },
                    onchange: function () {
                        T.setValue(D.year + "-" + D.month + "-" + D.day);
                        T.check();
                        D.remove();
                        D = null;
                        DateOpen = false;
                        T.err = false;
                        T.setfocus()
                    }
                })
            }
        });
    T.check = function () {
            var V = B.value.verification({
                maxLen: S.maxLen,
                minLen: S.minLen,
                minValue: S.minValue,
                maxValue: S.maxValue,
                type: "date"
            });
            if (S.repeat != null) {
                if ($(S.repeat).value != T.val()) {
                    V.R = false
                }
            }
            if (!V.R) {
                B.className = "TextBox_focus";
                T.err = true;
                if (S.errMsg != null && S.errMsg != "") {
                    T.errMsg = S.errMsg
                } else {
                    T.errMsg = V.M
                }
                ShowMsg(T.errMsg)
            } else {
                B.className = "TextBox_focus";
                T.err = false;
                T.errMsg = ""
            }
        };
    var Msg = null;
    var ShowMsg = function (text) {
            if (Msg == null) {
                Msg = $B.addControl({
                    xtype: 'Tip',
                    HTML: text,
                    className: 'M4_TipErrBox'
                });
                Msg.setXY(B.getXY().left + 30, B.getXY().top + 30)
            }
            var hide = function () {
                if (Msg != null) {
                    Msg.remove();
                    Msg = null
                }
            };
            setTimeout(hide, 1500)
        };
    var HideMsg = function () {};
    B.addEvent("onkeyup", function () {
            T.check()
        });
    this.setSize = function (w, h) {
            A.style.width = w + "px";
            B.style.width = (w - 21) + "px"
        };
    this.remove = function () {
            B.remove();
            C.remove();
            A.remove()
        };
    this.focus = function () {
            B.focus()
        };
    this.setfocus = function () {
            B.focus()
        };
    this.setValue = function (v) {
            if (v == "") {
                B.value = "";
                return
            }
            var Da = null;
            try {
                Da = v.toDate();
                B.value = Da.format(S.format)
            } catch (x) {
                B.value = ""
            }
            if (S.onchange) S.onchange()
        };
    this.val = function () {
            return (B.value)
        };
    if (S.value != null) this.setValue(S.value);
    this.setAttribute = function (a, b) {
            S[a] = b
        };
    this.focus()
};
$M.Control["Progress"] = function (BoxID, S) {
    var T = this;
    this.value = 0;
    var A = BoxID.addDom("div");
    A.className = "M4_Progress";
    if (S.width != null) A.style.width = S.width + "px";
    var B = A.addDom("div");
    B.className = "Progress";
    var D = B.addDom("div");
    D.className = "text";
    var C = B.addDom("div");
    C.className = "bar";
    if (S.value != null) {
        C.style.width = S.value + "%";
        D.innerHTML = "已完成" + S.value + "%";
        this.value = S.value
    }
    var xy = B.getXY();
    D.style.width = B.offsetWidth + "px";
    this.setValue = function (Value) {
        C.style.width = Value + "%";
        Value = Math.floor(Value);
        D.innerHTML = "已完成" + Value + "%";
        T.value = Value
    };
    this.moveTo = function (Value, Back) {
        var t = 0,
            d = 100;
        var U = function () {
                t++;
                var H = $.Tween.Linear(t, T.value, Value - T.value, d);
                T.setValue(H);
                if (t < d) setTimeout(U, 10);
                else {
                    T.setValue(Value);
                    if (Back != null) Back()
                }
            };
        U()
    }
};
$M.Control["TrackBar"] = function (BoxID, S) {
    var T = this;
    var Tag = false;
    var xz = 0,
        oldValue = null,
        objID = "TrackBar_" + $.Index + "_";
    var Button = objID + "0";
    this.value = 0;
    var A = BoxID.addDom("div");
    A.className = "M4_TrackBar";
    if (S.width != null) A.style.width = S.width + "px";
    A.tabIndex = 1;
    var html = "<div class='left' unselectable='on'><div class='right' unselectable='on'><div class='C' unselectable='on'><div class='button' unselectable='on' id='" + Button + "'></div></div></div></div>";
    A.innerHTML = html;
    Button = $(Button);
    T.addEvent = function (E, F) {
            S[E] = F
        };
    T.setValue = function (value) {
            value = Math.floor(value);
            var xy = A.getXY();
            if (value == null) value = S.min;
            if (value > S.max) value = S.max;
            if (value < S.min) value = S.min;
            var bfb = (A.offsetWidth - 10) / 100;
            if (S.max == S.min) {
                Button.style.left = "0px"
            } else {
                Button.style.left = (bfb * ((value - S.min) / ((S.max - S.min) / 100)) - 5) + "px"
            }
            this.value = value
        };
    T.setAttribute = function (a, b) {
            S[a] = b
        };
    A.addEvent("onkeydown", function () {
            if ($.event.keyCode() == 37) T.setValue(T.value - 1);
            if ($.event.keyCode() == 39) T.setValue(T.value + 1)
        });
    A.addEvent("onclick", function () {
            var x = $.event.x();
            var xy = A.getXY();
            var bfb = (A.offsetWidth - 10) / 100;
            var value = ((x - xy.left - (Button.offsetWidth / 2)) / bfb) * ((S.max - S.min) / 100) + S.min;
            oldValue = T.value;
            T.setValue(value);
            if (S.onchange != null && oldValue != T.value) S.onchange(T.value);
            A.focus()
        });
    Button.addEvent("onmouseout", function () {
            Button.className = "button"
        });
    Button.addEvent("onmousedown", function () {
            oldValue = T.value;
            Button.className = "button_down";
            var x = $.event.x();
            xz = x - Button.offsetLeft;
            Button.setCapture();
            Tag = true
        });
    Button.addEvent("onmousemove", function () {
            Button.className = "button_move";
            if (Tag) {
                var x = $.event.x();
                var xy = A.getXY();
                var WZ = 0;
                if (x > A.offsetWidth + xy.left - Button.offsetWidth) {
                    WZ = A.offsetWidth + xy.left - Button.offsetWidth
                } else if (x < xy.left) {
                    WZ = xy.left
                } else {
                    WZ = x - xz
                }
                var bfb = (A.offsetWidth - 10) / 100;
                var value = ((WZ - xy.left) / bfb) * ((S.max - S.min) / 100) + S.min;
                T.setValue(value)
            }
        });
    Button.addEvent("onmouseup", function () {
            Tag = false;
            Button.releaseCapture();
            Button.className = "button";
            if (S.onchange != null && oldValue != T.value) S.onchange(T.value)
        });
    if (S.value == null) this.setValue(S.min);
    else {
            this.setValue(S.value)
        }
};
$M.Control["Menu"] = function (BoxID, S) {
    var T = this;
    var A = null;
    if (!S.event) S.event = {};
    var keydown = function (e) {
        if (e.keyCode == 27) T.close();
        var $items = A.find("li a");
        if (!$items.length || (e.keyCode != 38 && e.keyCode != 40)) return
        var index = $items.index($items.filter(':focus'))
        if (e.keyCode == 38) {
            index = (index > 0) ? index - 1 : $items.length - 1;                        // up
        }
        if (e.keyCode == 40) {
            index = (index < $items.length - 1) ? index + 1 : 0;                        // down
        }
        if (! ~index) index = 0
        $items.eq(index).focus()
    };
    T.on = function (e, f) {
        S.event[e] = f;
    };
    var getMenuHtml = function (items) {
        var html = "";
        for (var i = 0; i < items.length; i++) {
            if (items[i].text == "-") {
                html += "<li class=\"divider\"></li>";
            } else {
                if (items[i].items) {
                    html += "<li class=\"dropdown-submenu\"><a href=\"#\">" + items[i].text + "</a>";
                    html += getMenuHtml(items[i].items);
                } else {
                    html += "<li><a href=\"#\">" + items[i].text + "</a>";
                }
                html += "</li>";
            }
        }
        html = "<ul class=\"dropdown-menu\" >" + html + "</ul>";
        return html;
    };
    T.open = function (x, y, obj) {
        if (obj != null) BoxID = obj;
        var html = getMenuHtml(S.items);
        A = $(html).appendTo(BoxID);


        if (x != null && y != null) A.css({ left: x + "px", top: y + "px" });
        $(document).on("keydown", keydown);
        $(document).on("mousedown", T.close);
    };
    T.close = function () {
        if (S.event.close != null && S.event.close() == false) return false;
        $(document).unbind("keydown", keydown);
        $(document).unbind("mousedown", T.close);
        if (A) A.remove();
    };
    $M.BaseClass.apply(T, [S]);
    return;
    var A = null,
        S1 = null;
    var T = this,
        xtag, ytag;
    T.tag = false;
    T.openTag = false;
    T.opensubTag = false;
    T.defaultMenu = null;
    T.client = null;
    T.parentMenu = S.parentMenu;
    T.items = S.item;
    this.reload = function () {
        S.item = T.items
    };
    var onmousedown = null;
    this.setAttribute = function (a, b) {
        S[a] = b
    };
    this.addLine = function () {
        var Item = A.addDom("div");
        Item.className = "Line"
    };
    this.addItem = function (I) {
        var T2 = this;
        var Item = A.addDom("div");
        Item.className = "Item";
        Item.unselectable = "on";
        var ICO = Item.addDom("div");
        ICO.className = "Ico";
        if (I.control == "CheckBox") ICO.addControl({
            xtype: "CheckBox",
            checked: I.checked
        });
        var Title = Item.addDom("div");
        Title.className = "Title";
        Title.unselectable = "on";
        if (I.item != null) {
            var Child = Item.addDom("div");
            Child.className = "Child"
        }
        T2.setCaption = function (title) {
            Title.innerHTML = title
        };
        T2.setCaption(I.caption);
        if (I.disabledTag) {
            Item.style.filter = "Alpha(Opacity=30)"
        } else {
            Item.style.filter = "Alpha(Opacity=100)"
        }
        Item.addEvent("onclick", function () {
            if (I.disabledTag) return;
            if (I.onclick != null) {
                I.onclick(I);
                T.closeMenu()
            }
        });
        T2.getXY = function () {
            return (Item.getXY())
        };
        Item.addEvent("onmouseover", function () {
            if (I.disabledTag) return;
            if (T.client != null) {
                T.opensubTag = false;
                T.client.remove();
                T.client = null
            }
            if (!T.opensubTag && T.defaultMenu != null) T.defaultMenu.className = "Item";
            Item.className = "Item Move";
            T.defaultMenu = Item;
            if (I.item != null && T.client == null) {
                var xy = A.getXY();
                var xy2 = Item.getXY();
                T.client = $B.addControl({
                    left: (xy.left + A.offsetWidth - 5),
                    top: 174,
                    xtype: "Menu",
                    width: I.width,
                    item: I.item,
                    parentMenu: T,
                    onfocus: function () {
                        T.opensubTag = true
                    },
                    onblur: function () {
                        T.opensubTag = false
                    }
                });
                T.client.show((xy.left + A.offsetWidth - 5), Item.getXY().top, T2, xtag, ytag)
            };
            T.tag = true
        });
        Item.addEvent("onmouseout", function () {
            if (!T.opensubTag) T.defaultMenu.className = "Item";
            T.tag = false
        })
    };
    this.show = function (l, t, obj, xtag2, ytag2) {
        if (S.onshow) S.onshow();
        xtag = xtag2;
        ytag = ytag2;
        var topX = 0,
                leftY = 0;
        if (obj != null) {
            var V = obj.getXY();
            leftX = V.left;
            topY = V.top
        }
        var bottomY = $B.scrollTop + $B.offsetHeight,
                rightX = $B.scrollLeft + $B.offsetWidth;
        T.opensubTag = false;
        if (t - 5 > 0 && l - 5 > 0) {
            S.top = t - 5;
            S.left = l - 5
        } else {
            S.top = t;
            S.left = l
        }
        S1 = BoxID.addControl({
            xtype: "Shadow",
            top: S.top,
            left: S.left
        });
        S1.setZIndex(10000);
        A = BoxID.addDom("div");
        A.className = "M4_Menu";
        var css = "top:" + S.top + "px;";
        css += "left:" + S.left + "px;z-index:10000;";
        A.cssText(css);
        for (var i = 0; i < T.items.length; i++) {
            if (T.items[i].caption == "-") T.addLine();
            else {
                new T.addItem(T.items[i])
            }
        }
        var width = A.offsetWidth,
                height = A.offsetHeight;
        if (ytag2) { } else {
            if ((t + height) > bottomY) {
                A.style.top = (t - height) + "px";
                ytag = true
            }
        }
        if (xtag2) {
            A.style.left = (leftX - width) + "px"
        } else {
            if ((l + width) > rightX) {
                if (obj != null) {
                    A.style.left = (leftX - width) + "px"
                } else {
                    A.style.left = (l - width) + "px"
                }
                xtag = true
            }
        }
        S1.setWidth(width);
        S1.setHeight(height);
        var xy = A.getXY();
        S1.setXY(xy.left, xy.top);
        if (S.onfocus != null) S.onfocus();
        A.addEvent("onmouseover", function () {
            T.tag = true
        });
        A.addEvent("onmouseout", function () {
            T.tag = false
        });
        onmousedown = function () {
            var parentMenu_opensubTag;
            if (S.parentMenu == null) parentMenu_opensubTag = false;
            else {
                parentMenu_opensubTag = S.parentMenu.opensubTag
            }
            if (!T.getClienttag() && !parentMenu_opensubTag) T.remove()
        };
        $D.addEvent("onmousedown", onmousedown)
    };
    this.remove = function () {
        if (T.client != null) {
            T.client.remove();
            T.client = null
        };
        if (A != null) {
            A.remove();
            A = null
        };
        if (S1 != null) {
            S1.remove();
            S1 = null
        };
        if (S.onclose != null) S.onclose();
        $D.removeEvent("onmousedown", onmousedown)
    };
    this.getClienttag = function () {
        var obj = T;
        var tag = false;
        while (obj != null) {
            tag = tag || obj.tag;
            obj = obj.client
        }
        return (tag)
    };
    this.closeMenu = function () {
        var obj = T,
                obj1 = obj;
        while (obj != null) {
            obj = obj.parentMenu;
            if (obj != null) obj1 = obj
        }
        obj1.remove()
    };
    this.regedit = {
        shortcutMenu: function (obj) {
            obj.addEvent("onmouseup", function () {
                if (!T.getClienttag() && $.event.button() == 2) T.show($.event.x(), $.event.y())
            })
        },
        controlMenu: function (obj) { }
    }
};


$M.Control["TreeView"] = function (BoxID, S) {
    var T = this;
    T.selectedItem = null;
    var A = $("<div class=\"tree\" tabindex=\"-1\" ></div>").appendTo(BoxID);
    if (!S.openIco) S.openIco = "fa fa-minus-square-o";
    if (!S.closeIco) S.closeIco = "fa fa-plus-square-o";
    var _setIco = function (item, _ico) {
        if (item.child.items.length > 0) {
            _ico.attr("class", item.child.isOpen ? S.openIco : S.closeIco);
        }
    }
    var _add = function (box, Sitem) {
        if (!Sitem) return;
        if (Sitem.length == null) {
            return new box.item(Sitem);
        } else {
            for (var i = 0; i < Sitem.length; i++) {
                new box.item(Sitem[i]);
            }
        }
    }
    var TreeChild = function (BoxID, items) {
        var T2 = this;
        T2.isOpen = true;
        var A = $("<ul></ul>").appendTo(BoxID);
        T2.items = [];
        T2.item = function (S2) {
            var T3 = this;
            T2.items[T2.items.length] = T3;
            T3.child = null;
            var line = "<li>";
            line += " <a href=\"#\" tabindex=\"-1\"><i/>" + S2.text + "</a>";
            line += "</li>";
            var box = $(line).appendTo(A);
            var _ico = $(box).find("i");
            var _title = $(box).find("a");
            $M.BaseClass.apply(T3, [S2]);
            T3.addClass = function (S3) { box.addClass(S3); };
            T3.removeClass = function (S3) { box.removeClass(S3); };
            T3.addItem = function (S3) {
                if (T3.child == null) T3.child = new TreeChild(box, null);
                var item = _add(T3.child, S3);
                _setIco(T3, _ico);
                return item;
            };
            T3.addClass(S2["class"]);
            T3.addItem(S2.items);
            _ico.click(function () {
                if (T3.child) {
                    if (T3.child.isOpen) {
                        T3.child.close();
                    } else {
                        T3.child.open();
                    }
                    _setIco(T3, _ico);
                }
            });
            _title.click(function () {
                if (T.selectedItem) T.selectedItem.removeClass("active");
                T.selectedItem = T3;
                T3.addClass("active");
                if (S.onAfterSelect) { S.onAfterSelect(T, { item: T3 }); };
            });
            T3.remove = function () {
                T3.child.remove();
                box.remove();
                T2.items = T2.items.del(T3);

                T3 = null;
            };
        };
        T2.clear = function () {
            for (var i = T2.items.length - 1; i > -1; i--) {
                T2.items[i].remove();
            }
        };
        T2.remove = function () {
            T2.clear();
            A.remove();
            T2 = null;
        };
        T2.addItem = function (S2) {
            return _add(T2, S2);
        };
        T2.close = function () {
            A.hide();
            T2.isOpen = false;
        };
        T2.open = function () {
            A.show();
            T2.isOpen = true;
        };
        T2.addItem(items);
    }
    T.root = new TreeChild(A, S.items);
    $M.BaseClass.apply(T, [S]);

    var keydown = function (e) {
        if (A[0] == $M.focusElement || A.has($M.focusElement).length) {
            if (S.onKeyDown) S.onKeyDown(T, e);
        }
    };
    $(document).on("keydown", keydown);
};