$M.Control["EditorColorBox"] = function (BoxID, S) {
    var T = this;
    var A = null;
    var color = null,backColor=null;
    var list = [["#000", "#444", "#666", "#999", "#ccc", "#eee", "#f3f3f3", "#fff"],
        ["#f00", "#f90", "#ff0", "#0f0", "#0ff", "#00f", "#90f", "#f0f"],
        ["#f4cccc", "#fce5cd", "#fff2cc", "#d9ead3", "#d0e0e3", "#cfe2f3", "#d9d2e9", "#ead1dc"],
        ["#ea9999", "#f9cb9c", "#ffe599", "#b6d7a8", "#a2c4c9", "#9fc5e8", "#b4a7d6", "#d5a6bd"],
        ["#e06666", "#f6b26b", "#ffd966", "#93c47d", "#76a5af", "#6fa8dc", "#8e7cc3", "#c27ba0"],
        ["#c00", "#e69138", "#f1c232", "#6aa84f", "#45818e", "#3d85c6", "#674ea7", "#a64d79"],
        ["#900", "#b45f06", "#bf9000", "#38761d", "#134f5c", "#0b5394", "#351c75", "#741b47"],
        ["#600", "#783f04", "#7f6000", "#274e13", "#0c343d", "#073763", "#20124d", "#4c1130"]];
    T.open = function (x, y, obj) {
        if (obj != null) BoxID = obj;
        A = $("<div class=\"M5_droppicker dropdown-menu\" ></div>").appendTo(BoxID);
        var html = "<div class=\"M5_Color btn-group\" style='float:left;'>";
        html += "<div class=\"note-palette-title\" >背景颜色</div>";
        html += "<div class=\"note-color-reset\" unselectable=\"on\" _t=0>设为透明</div>";
        for (var i = 0; i < list.length; i++) {
            html += "<div class=\"color-row\">";
            for (var i1 = 0; i1 < list[i].length; i1++) {
                html += "<button type=\"button\" class=\"color-btn\" style=\"background-color:" + list[i][i1] + ";\"  _t=0 val=\"" + list[i][i1] + "\"  />";
            }
            html += "</div>";
        }
        html += "</div>";

        html += "<div class=\"M5_Color btn-group\"  style='float:left;'>";
        html += "<div class=\"note-palette-title\" >前景颜色</div>";
        html += "<div class=\"note-color-reset\" unselectable=on _t=1>设为默认颜色</div>";
        for (var i = 0; i < list.length; i++) {
            html += "<div class=\"color-row\">";
            for (var i1 = 0; i1 < list[i].length; i1++) {
                html += "<button type=\"button\" class=\"color-btn\" style=\"background-color:" + list[i][i1] + ";\" _t=1 val=\"" + list[i][i1] + "\"  />";
            }
            html += "</div>";
        }
        html += "</div>";
        A.html(html);
        A.find("button").click(function () {
            if (S.onChanage) S.onChanage(T, { type: $(this).attr("_t"), color: $(this).attr("val") });
            T.close();
        });
        A.find(".note-color-reset").click(function () {
            if (S.onChanage) S.onChanage(T, { type: $(this).attr("_t"), color:""});
            T.close();
            return false;
        });
        if (x != null && y != null) A.css({ left: x + "px", top: y + "px", display: "inline" });
        $(document).on("mousedown", function (e) {
            if (A.has(e.target).length == 0) T.close();
        });
    };
    T.val = function () { return color; };
    T.close = function () {
        if (S.onClose) S.onClose();
        //$(document).unbind("keydown", keydown);
        $(document).unbind("mousedown", T.close);
        if (A) A.remove();
    };
    $M.BaseClass.apply(T, [S]);
};
$M.Control["Editor"] = function (BoxID, S, CID) {
    var T = this;
    var A = null;
    if (CID != null) A = CID;
    else { A = $("<div class=\"M5_Editor\"></div>").appendTo(BoxID); }

    var content, codeBox;
    var colorButton = null;
    var colorMenu = $('body').addControl({
        xtype: "EditorColorBox", onChanage: function (sender, e) {
            if (e.type == 0) {
                colorButton.$("i").css({ color: e.color });
                execComm('ForeColor', e.color);
                colorButton.attr("_color", e.color);
            } else {
                colorButton.$("i").css({ "background-color": e.color });
                execComm('BackColor', e.color);
                colorButton.attr("_backColor", e.color);
            }
        }
    });
    var isCodeMode = false;
    var getSelection = function () {
        var V = {};
        var selection = ifr.contentWindow.getSelection();
        if (selection.getRangeAt.length > 0) {
            var range = selection.getRangeAt(0);
            if (range.startContainer.nodeType == 1) {
                V.type = "control";
                V.obj = selection.anchorNode.childNodes[selection.anchorOffset];
                if (V.obj && V.obj.tagName) V.html = getOuterHTML(V.obj);
            } else {
                V.type = "html";
                var range = selection.getRangeAt(0);
                var container = document.createElement('div');
                container.appendChild(range.cloneContents());
                V.text = V.selection.toString();
                V.html = container.innerHTML;
            }
        } else {
            V.type = "html";
            var range = selection.getRangeAt(0);
            var container = document.createElement('div');
            container.appendChild(range.cloneContents());
            V.text = selection.toString();
            V.html = container.innerHTML;
        };
        return V;
    };
    var insertTag = function (name) {
        var sel = getSelection();
        alert(sel.type);
        return;
        if (name == "") {
            execComm("RemoveFormat");
        } else {
            html = "<" + tagname + ">" + S.html + "</" + tagname + ">";
            T.setDocumentFocus();
            T.insertHtml(html);
        }
    };
    var setCodeMode = function (flag) {
        if (flag == isCodeMode) return;
        if (flag) {
            content.hide();
            codeBox.show();
            codeBox.val(T.val());
        } else {
            content.show();
            codeBox.hide();
            content[0].innerHTML = T.val();
        }
        S.sourceMode = flag;
        isCodeMode = flag;
    };
    var execComm = function (c, p) { document.execCommand(c, null, p); };
    var toolbar = A.addControl({
        xtype: "ToolBar", size: S.size == null ? 1 : S.size, "class": "note-toolbar", items: [[{
            text: "普通", menu: [
                { text: "<h1>H1</h1>" },
                { text: "<h2>H2</h2>" },
	            { text: "<h3>H3</h3>" },
	            { text: "<h4>H4</h4>" },
	            { text: "<h5>H5</h5>" },
	            { text: "<h6>H6</h6>" }
	        ]
        }],
        [{ ico: "fa-bold", onClick: function () { execComm('Bold'); } }, { ico: "fa-italic", onClick: function () { execComm('Italic'); } }, { ico: "fa-underline", onClick: function () { execComm('Underline'); return false; } }],
        [{ ico: "fa-align-left", onClick: function () { execComm('JustifyLeft'); } }, { ico: "fa-align-center", onClick: function () { execComm('JustifyCenter'); } }, { ico: "fa-align-right", onClick: function () { execComm('JustifyRight'); } }, { ico: "fa-align-justify", onClick: function () { execComm('JustifyNone'); return false; } }],
        [{ ico: "fa-list-ol", onClick: function () { execComm('InsertOrderedList'); } }, { ico: "fa-list-ul", onClick: function () { execComm('InsertUnorderedList'); return false; } }],
        [{ ico: "fa-file-image-o", onClick: function () {
            $M.app.call("$M.system.insertPic", { back: function (json) {
                var html = "";
                for (var i = 0; i < json.length; i++) {
                    html += "<img src='" + json[i] + "' >";
                }
                T.insertHtml(html);
            }
            });
            return false;
        }
        }, { ico: "fa-file-movie-o" }, { ico: "fa-file-word-o"}],
        [{ ico: "fa-link" }, { name: "colorC", ico: "fa-font", menu: colorMenu, onClick: function (sender) { execComm('ForeColor', colorButton.attr("_color")); execComm('BackColor', colorButton.attr("_backColor")); return false; } }],
        [{ ico: "fa-code", onClick: function () { setCodeMode(!isCodeMode); return false; } }]
	    ]
    });

    colorButton = toolbar.find("colorC");
    content = $("<div class=\"note-editable\" contenteditable=\"true\"></div>").appendTo(A);
    content.keydown(function (e) {
        if (e.which == 13) {
            execComm('formatblock', '<p>');
        }
    });
    var codeBox = $("<textarea/>").appendTo(A);
    //codeBox = CodeMirror.fromTextArea(code[0], {
    //    mode: "text/html",
    //    lineNumbers: true
    //});
    codeBox.hide();
    T.val = function (html) {
        if (html!=null) {
            if (isCodeMode) {
                codeBox.val(html);
            } else {
                content[0].innerHTML = html;
            }
        }
        var html = "";
        if (isCodeMode) {
            html = codeBox.val();
        } else {
            html = content[0].innerHTML;
        }
        return html;
    };
    var documentFocus = null, documentCodeFocus = null;
    T.focus = function () {
        if (isCodeMode) {
            //if (ieTag) {
            codeBox.focus();
            //	            setCaret(codeBox, DocumentCodeFocus);
            //}
        } else {
            if (documentFocus != null) {

                var sel = window.getSelection();
                sel.removeAllRanges();
                sel.addRange(documentFocus.range);
                //documentFocus = null;
            } else { }
            //content.focus();
        }
    };

    T.getFocus = function () {
        documentFocus = null;
        documentCodeFocus = null;
        var V = null;
        if (isCodeMode) {
            if ($.browser.msie) {
                DocumentCodeFocus = getPos(codeBox);
            } else {
                //alert(codeBox.getSelection());
                //alert(codeBox.selection);

                //var R=codeBox.getSelection();
                //if(R.getRangeAt.length>0){
                //    DocumentCodeFocus=R.getRangeAt(0);
                //    alert(1);
                //}
            }
        } else {
            documentFocus = {};
            if ($.browser.msie) {
                //alert(document.selection);
                //selection = content.selection;
                documentFocus.sel = document.selection;
                documentFocus.range = documentFocus.sel.createRange();
                //V.selection = document.selection;
                //var oRange = V.selection.createRange();
                if (documentFocus.sel.type == "Control") {
                    if (documentFocus.range) documentFocus.obj = documentFocus.range.item(0);
                    documentFocus.type = "Control";
                    documentFocus.html = documentFocus.obj.outerHTML;
                } else {
                    if (documentFocus.range) {
                        documentFocus.type = "Text";
                        documentFocus.text = documentFocus.range.text;
                        documentFocus.html = documentFocus.range.htmlText;
                    }
                }
            } else {
                documentFocus.sel = window.getSelection();
                documentFocus.range = documentFocus.sel.getRangeAt(0);
                //                alert(window.getSelection());
                //documentFocus.sel = ifr.contentWindow.getSelection();
            }
        }
        return V;
    };
    var insertText = function (html) {
        var oValue = codeBox[0].value;
        var startIndex = 0;
        var selLength = 0;
        var scrollTop = codeBox.scrollTop;
        startIndex = codeBox[0].selectionEnd;
        selLength = codeBox[0].selectionEnd - codeBox[0].selectionStart;
        var fValue = oValue.substring(0, startIndex - selLength);
        var eValue = oValue.substr(startIndex);
        codeBox[0].value = fValue + html + eValue;
        codeBox[0].focus();
        codeBox[0].setSelectionRange(startIndex - selLength, startIndex - selLength + html.length);
        //codeBox[0].scrollTop = scrollTop;
    };
    T.insertHtml = function (html) {
        T.focus();
        if ($.browser.msie) {
        } else {
            if (isCodeMode) {
                insertText(html);
            } else {
                execComm("InsertHtml", html);
            }
        }

    };
    T.container = A;
    $M.BaseClass.apply(T, [S]);
    T.css = function (style) {
        if (style) {
            A.css(style);
            content.outerHeight(A.height() - toolbar.height());
            codeBox.css({ "width": A.width() + "px", "height": (A.height() - toolbar.container.outerHeight()) + "px" });
            //codeBox.setSize(A.width(), content.height());

            //if (style && style.height) {
            //}
        }
    };
    T.css(S.style);
    if (S.sourceMode != null) setCodeMode(S.sourceMode);
    return;







    if (S.width != null || S.height != null) {
        A.setSize(S.width, S.height);
    }
    T.editorMode = 0;
    T.err = false;
    var bodycss = "edithtmlbody.css";
    if (S.bodyCss != null) bodycss = S.bodyCss;
    var editFocus = false;
    var Labels = new Array();
    var HeadHtml = "", EndHtml = "", CssStr = "";
    var Selection = null;
    var DocumentFocus = null, DocumentCodeFocus = null;
    var ToolBar = A.addControl({ xtype: "ToolBar", unselectable: "on" });

    if (S.baseTool != true) {
        ToolBar.add({
            xtype: "SelectBox",
            width: 80, value: "",
            items: [
	        { text: "普通", value: "" },
	        { text: "H1", value: "H1" },
	        { text: "H2", value: "H2" },
	        { text: "H3", value: "H3" },
	        { text: "H4", value: "H4" },
	        { text: "H5", value: "H5" },
	        { text: "H6", value: "H6" }
	        ], onchange: function (obj) { if (obj.getValue != null) T.insertH(obj.getValue()); }
        });
    }
    ToolBar.add({
        xtype: "SelectBox",
        width: 80, value: "",
        items: [
	        { text: "普通", value: "" },
	        { text: "1(8磅)", value: "1" },
	        { text: "2(10磅)", value: "2" },
	        { text: "3(12磅)", value: "3" },
	        { text: "4(14磅)", value: "4" },
	        { text: "5(16磅)", value: "5" },
	        { text: "6(18磅)", value: "6" },
	        { text: "7(20磅)", value: "7" }
	        ], onchange: function (obj) { if (T.command != null) T.command('fontsize', obj.getValue()); }
    });
    ToolBar.add({ ico: "htmlEdit_B", onclick: function () { T.command("Bold"); } });
    ToolBar.add({ ico: "htmlEdit_I", onclick: function () { T.command("Italic"); } });
    ToolBar.add({ ico: "htmlEdit_U", onclick: function () { T.command("Underline"); } });
    if (S.baseTool != true) {
        ToolBar.add({ caption: "|" });
        ToolBar.add({ ico: "htmlEdit_Left", onclick: function () { T.command("JustifyLeft"); } });
        ToolBar.add({ ico: "htmlEdit_Center", onclick: function () { T.command("JustifyCenter"); } });
        ToolBar.add({ ico: "htmlEdit_Right", onclick: function () { T.command("JustifyRight"); } });
        ToolBar.add({ ico: "htmlEdit_None", onclick: function () { T.command("JustifyNone"); } });
        ToolBar.add({ caption: "|" });
        ToolBar.add({ ico: "htmlEdit_OrderedList", onclick: function () { T.command("InsertOrderedList"); } });
        ToolBar.add({ ico: "htmlEdit_UnorderedList", onclick: function () { T.command("InsertUnorderedList"); } });
        ToolBar.add({ caption: "|" });
        var Menu = A.addControl({ xtype: "Menu", width: 140, item: [
					    { caption: "多文件上传", onclick: function () {
					        T.getDocumentFocus();
					        dialog.uploadFiles({
					            file_types: "*.jpg;*.gif;*.png",
					            file_types_description: "全部图片",
					            back: function (file, obj) {
					                if (obj.value == 1) T.insertHtml("<p><img src='" + obj.uploaddir + obj.path + "' /></p>");
					                //alert(1);
					            }
					        });

					    }
					    }
				    ]
        });
        var colorBox = $B.addControl({ xtype: "ColorBox", onselect: function (v) { T.command('ForeColor', v); } });
        var colorBox2 = $B.addControl({ xtype: "ColorBox", onselect: function (v) { T.command('BackColor', v); } });


        ToolBar.add({ ico: "htmlEdit_Img", unselectable: "no", onclick: function () { T.insertImg(); }, menu: Menu });
        var insertVoed = function (tag) {
            T.getDocumentFocus();
            dialog.input({ caption: "视频所在网址", back: function (V) {
                Ajax("getVideo", [tag, V.text], function (xml) {
                    T.insertHtml(xml[0].text);
                });
            }
            });
        };
        var vmenu = A.addControl({ xtype: "Menu", width: 140, item: [
	                    { caption: "优酷", onclick: function () { insertVoed(0); } },
	                    { caption: "酷6", onclick: function () { insertVoed(1); } }
	                    ]
        });
        ToolBar.add({ ico: "htmlEdit_Video", menu: vmenu, onclick: function () {
            T.getDocumentFocus();
            dialog.input({ caption: "视频地址", back: function (V) {
                if (V.tag) {
                    var html = '<object   align=middle   classid=CLSID:22d6f312-b0f6-11d0-94ab-0080c74c7e95  class=OBJECT  >' +
                      '<param   name=ShowStatusBar   value=-1>' +
                      '<param   name=Filename   value="' + V.text + '">' +
                      '<embed   type=application/x-oleobject   codebase=http://activex.microsoft.com/activex/controls/mplayer/en/nsmp2inf.cab#Version=5,1,52,701   flename=mp   src="' + V.text + '"   width=400   height=300>' +
                      '</embed></object>';
                    T.insertHtml(html);
                }
            }
            });
        }
        });
    }
    ToolBar.add({ ico: "htmlEdit_Link", onclick: function () { T.insertLink(); } });
    ToolBar.add({ ico: "htmlEdit_Color", menu: colorBox });
    ToolBar.add({ ico: "htmlEdit_backColor", menu: colorBox2 });
    ToolBar.add({ ico: "htmlEdit_Format", onclick: function () { formatHtml(); } });
    ToolBar.add({ ico: "htmlEdit_Html", onclick: function () {
        var R = selectObj();
        if (R.type != null) {
            T.getDocumentFocus();
            dialog.editHtml({ value: getObjectHtml(), back: function (V) {
                T.setDocumentFocus();
                if (V.tag) T.insertHtml(V.text);
            }
            });
        } else { dialog.alert({ msg: "没有选择对象!" }); }
    }
    });
    ToolBar.add({ ico: "htmlEdit_PageSpacer", onclick: function () {
        var sel = selectObj();
        var PObj = null;
        if (sel.type == "Control") {
        } else {
            var B = sel.getParentElement();
            while (B && B.nodeName != "P") B = B.parentNode;
            if (B && B.nodeName == "P") PObj = B;

            if (PObj != null) {
                if (PObj.parentNode != null) {
                    var c = PObj.parentNode.childNodes.length;
                    var index = 0;
                    for (var i = 0; i < c; i++) {
                        if (PObj.parentNode.childNodes[i] == PObj) {
                            index = i;
                        }
                    }
                    if (index + 1 < PObj.parentNode.childNodes.length) {
                        index++;
                    } else {
                        index = PObj.parentNode.childNodes.length - 1;
                    }
                    var Index = Labels.length;
                    Labels[Index] = { html: "<!-- PageSpacer -->" };
                    var obj = HtmlBox.createElement("img");
                    obj.id = "SystemPic_" + Index;
                    obj.src = "/manage/skin/icon/blank.gif";
                    obj.className = "Label PageSpacerIco";
                    PObj.parentNode.insertBefore(obj, PObj.parentNode.childNodes[index]);
                }
            } else {
                T.insertHtml(replaceLabel("<!-- PageSpacer -->"));
            }
        }
    }
    });
    var Box = A.addDom("div"); Box.className = "HtmlBox";
    var End = A.addDom("div"); End.className = "M4_ToolBar"; End.unselectable = "on";
    var ButtonCheck = End.addControl({ xtype: "ButtonCheckGroup", items: [
				        { caption: "设计", value: 0, checked: true },
				        { caption: "代码", value: 1}]
    });
    var ifr = null, codeBox = null;

    if (ieTag && !$.Browse.isIE9()) {
        ifr = Box.addDom("<iframe style='width:100%;height:100%;' frameborder='0'/>");
    } else {
        ifr = Box.addDom("iframe");
        ifr.setAttribute("frameborder", "0");
        ifr.cssText("width:100%;height:100%;");
    }
    var codeBox = Box.addDom("textarea"); codeBox.cssText("overflow:auto;width:100%;height:100%;");
    var HtmlBox = ifr.contentWindow.document;
    this.name = S.name;
    HtmlBox.designMode = "on";
    T.insertH = function (tagname) {
        T.getDocumentFocus();
        var S = selectObj();
        var linkObj = null;
        //        if(S.obj!=null){
        //        alert(S.obj.tagName.indexOf("H"));
        //		    if(S.obj.tagName.indexOf("H")==0)linkObj=S.obj;
        //		}else{
        //var B=S.getParentElement();
        //alert([B,B.nodeName.length==2,B.nodeName.indexOf("H")==0]);
        //while (B!=null&&B.nodeName.length==2&&B.nodeName.indexOf("H")==0){linkObj=B;B=B.parentNode;}
        //if(B!=null&&B.nodeName.length==2&&B.nodeName.indexOf("H")==0)linkObj=B;

        //		}
        if (tagname == "") {
            T.command("RemoveFormat");
        } else {
            html = "<" + tagname + ">" + S.html + "</" + tagname + ">";
            T.setDocumentFocus();
            T.insertHtml(html);
        }

    };
    T.insertLink = function () {
        T.getDocumentFocus();
        var S = selectObj();
        dialog.insertLink(S, function (html) {
            T.setDocumentFocus();
            T.insertHtml(html);
        });
    };
    T.check = function () {
    }
    T.resize = function () {
        Box.setSize(null, A.offsetHeight - ToolBar.height - End.offsetHeight);
    }
    T.setSize = function (width, height) {
        A.setSize(width, height);
        T.resize();
    }
    T.command = function (text, C) {
        HtmlBox.execCommand(text, false, C);
    }
    T.setAttribute = function (A, B) { S[A] = B; }
    var selectObj = function () {
        if (T.editorMode == 1) return;
        var V = new Object, R = null, oRange = null;
        if (ieTag) {
            V.selection = HtmlBox.selection;
            var oRange = V.selection.createRange();
            if (V.selection.type == "Control") {
                if (oRange) V.obj = oRange.item(0);
                V.type = "Control";
                V.html = V.obj.outerHTML;
            } else {
                if (oRange) {
                    V.type = "Text";
                    V.text = oRange.text;
                    V.html = oRange.htmlText;
                }
            }
        } else {



            V.selection = ifr.contentWindow.getSelection();
            if (V.selection.getRangeAt.length > 0) {
                var range = V.selection.getRangeAt(0);
                //alert(range.startContainer.nodeType);
                if (range.startContainer.nodeType == 1) {
                    V.type = "Control";
                    V.obj = V.selection.anchorNode.childNodes[V.selection.anchorOffset];
                    if (V.obj && V.obj.tagName) V.html = getOuterHTML(V.obj);
                } else {
                    V.type = "Html";
                    var range = V.selection.getRangeAt(0);
                    var container = document.createElement('div');
                    container.appendChild(range.cloneContents());
                    V.text = V.selection.toString();
                    V.html = container.innerHTML;
                }
            } else {
                V.type = "Html";
                var range = V.selection.getRangeAt(0);
                var container = document.createElement('div');
                container.appendChild(range.cloneContents());
                V.text = V.selection.toString();
                V.html = container.innerHTML;
            }
        }
        V.getParentElement = function () {
            if (ieTag) return (oRange.parentElement());
            else {
                if (R) {
                    var B = R.anchorNode;
                    while (B && B.nodeType != 1) B = B.parentNode;
                    return B;
                }
                return null;
            }
        }
        return (V);
    };
    T.insertImg = function () {
        T.getDocumentFocus();
        var V = selectObj();
        if (V.obj != null) {
            if (V.obj.tagName == "IMG") {
                dialog.picAttribute(V.obj, function (html) {
                    T.setDocumentFocus();
                    T.insertHtml(html);
                });
            }
        } else {
            dialog.picAttribute(null, T.insertHtml, T);
        }
    }
    var getOuterHTML = function (obj) {
        var canHaveChildren = function (obj) {
            return !/^(area|base|basefont|col|frame|hr|img|br|input|isindex|link|meta|param)$/.test(obj.tagName.toLowerCase());
        };
        var a = obj.attributes, str = "<" + obj.tagName, i = 0;
        if (a != null) {
            for (; i < a.length; i++)
                if (a[i].specified) str += " " + a[i].name + '="' + a[i].value + '"';
        }
        if (!canHaveChildren(obj)) return str + " /> ";
        return str + ">" + obj.innerHTML + "</" + obj.tagName + ">";
    };
    var getObjectHtml = function () {
        var Html = "";
        var V = selectObj();
        if (V == null) return (Html);
        Html = V.html;
        Html = (Html + "").ltrim();
        Html = getHtmlValue(Html);
        Html = replaceValue(Html);
        return (Html);
    }
    T.insertHtml = function (html) {
        T.setDocumentFocus();
        var sel = selectObj();
        var insert = function (html) {
            var B = sel;
            if (ieTag) {
                if (B.type == "Control") {
                    B.obj.outerHTML = html;
                } else {
                    B.selection.createRange().pasteHTML(html);
                }
            } else {
                HtmlBox.body.focus();
                if (B == null) T.command("InsertHtml", html);
                else {
                    for (var i = 0; i < B.selection.rangeCount; i++) {
                        B.selection.getRangeAt(i).deleteContents();
                    };
                    T.command("InsertHtml", html);
                }
            }
        };
        if (T.editorMode == 1) {
            var oValue = codeBox.value;
            var startIndex = 0;
            var selLength = 0;
            var scrollTop = codeBox.scrollTop;
            var strlen = function (str) {
                return (/ie/i.test(navigator.appVersion) && str.indexOf('\n') != -1)
                  ? str.replace(/\r?\n/g, '_').length : str.length;
            };
            if (ieTag) {
                var Sel = document.selection.createRange();
                Sel.text = html;
                Sel.moveStart('character', -strlen(html));
                Sel.select();
            } else {
                startIndex = codeBox.selectionEnd;
                selLength = codeBox.selectionEnd - codeBox.selectionStart;
                var fValue = oValue.substring(0, startIndex - selLength);
                var eValue = oValue.substr(startIndex);
                codeBox.value = fValue + html + eValue;
                codeBox.focus();
                codeBox.setSelectionRange(startIndex - selLength, startIndex - selLength + html.length);
                codeBox.scrollTop = scrollTop;
            }
        }
        if (T.editorMode == 0) {
            html = replaceLabel(html);
            html = replaceUrls(html);
            html = replaceEvents(html);
            //HtmlBox_body.focus();
            insert(html);
        }
    }
    T.setValue = function (value) {
        if (T.editorMode == 0) {
            value = getHtmlCoding(value);
        }
        initMode(T.editorMode);
        if (T.editorMode == 0) HtmlBox.body.innerHTML = value;
        else { codeBox.value = value; }
        T.value = value;
    }
    T.getValue = function () {
        switch (T.editorMode) {
            case 0:
                var html = HtmlBox.body.innerHTML;
                html = getHtmlValue(html);
                html = replaceValue(html);
                value = HeadHtml + html + EndHtml;
                break;
            case 1:
                value = codeBox.value;
                break;
        }
        return (value);
    }
    var getHtmlValue = function (Html) {
        //有问题代码
        Html = Html.replace(/HtmlEvent_/gi, "");
        //Html=Html.replace(/ href=\"[^(\")]*\"/gi,"")
        //Html=Html.replace(/ src=\"[^(\")]*\"/gi,"")
        //Html=Html.replace(/HtmlUrl_/gi,"");
        //Html=replaceValue(Html);
        var _Replace = function (V) {
            V = V.replace(/ href=\"[^(\")]*\"/gi, "")
            V = V.replace(/ src=\"[^(\")]*\"/gi, "")
            V = V.replace(/HtmlUrl_/gi, "");
            return (V);
        }
        Html = Html.replace(/<([^>]*)HtmlUrl_([^>]*)>/gi, _Replace);
        return (Html);
    }
    var getHtmlCoding = function (Html) {
        r = Html.match(new RegExp(/<\/body>[^~]*/gi));
        if (r != null) EndHtml = r[0];
        else { EndHtml = ""; }
        r = Html.match(new RegExp(/^[^~]*<body([^>]*)>/gi));
        if (r != null) HeadHtml = r[0];
        else { HeadHtml = ""; }
        r = Html.match(new RegExp(/<link[^>]*>/gi));
        CssStr = "";
        if (r != null) {
            for (var n = 0; n < r.length; n++) {
                CssStr = CssStr + r[n];
            }
        }

        Html = Html.replace(/<\/body>[^*]*/gi, "").replace(/^[^*]*<body([^>]*)>/gi, "");
        Html = replaceLabel(Html);

        Html = replaceUrls(Html);

        Html = replaceEvents(Html);
        return (Html);
    }
    var replaceEvents = function (A) {
        function _Replace(m, opener, index) {
            return ("HtmlEvent_" + m);
        };
        A = A.replace(/(on\w+)[\s\r\n]*=/gi, _Replace);
        return (A);
    }
    var replaceUrls = function (A) {
        A = A.replace(/<(a|link|area)(?=\s).*?\shref=((?:("|').*?("|')\2)|(?:[^"'][^ >]+))/gi, '$& HtmlUrl_href=$2');
        A = A.replace(/<(img|input|object)(?=\s).*?\ssrc=((?:("|').*?("|')\2)|(?:[^"'][^ >]+))/gi, '$& HtmlUrl_src=$2');
        return (A);
    }
    var focus = function () {
        editFocus = true;
    }
    var blur = function () {

        editFocus = false;
    }
    ifr.addEvent("onfocus", focus);
    ifr.addEvent("onblur", blur);
    codeBox.addEvent("onfocus", focus);
    codeBox.addEvent("onblur", blur);
    var DesignInit = function () {
        HtmlBox.open();
        HtmlBox.write('<!DOCTYPE html PUBLIC "-\/\/W3C\/\/DTD XHTML 1.0 Transitional\/\/EN" "http:\/\/www.w3.org\/TR\/xhtml1\/DTD\/xhtml1-transitional.dtd"><html><head><link rel="stylesheet" type="text\/css" href="skin\/css\/' + bodycss + '" \/>' + CssStr + '<meta http-equiv="Content-Type" content="text\/html; charset=gb2312" \/></head><body ><\/body><\/html>');
        HtmlBox.close();
        var body = $.Function.applyObj(HtmlBox.body);
        var Document = $.Function.applyObj(HtmlBox);
        var keyEvent = function (e) {
            var keycode = null;
            keycode = ieTag ? e.keyCode : e.which;
            if (keycode == 13) {
                HtmlBox.execCommand('formatblock', false, '<P>');
                //T.insertHtml("<p>　</p>");return(false);
            }

        };
        if (ieTag) {
            body.addEvent("onkeydown", keyEvent);
        } else {
            Document.addEvent("onkeydown", keyEvent);
        }
        editFocus = false;
        //HtmlBox_body.addEvent("onfocus",focus);
        //HtmlBox_body.addEvent("onmouseup",focus);
        //HtmlBox_body.addEvent("onblur",blur);
        HtmlBox.addEvent("ondblclick", function () {
            var R = selectObj();
            if (R.type != null) {
                T.getDocumentFocus();
                //dialog.picAttribute(null,T.insertHtml);
                if (S.onopenobj != null) {
                    if (R.obj != null) {
                        Html = R.html;
                        Html = getHtmlValue(Html);
                        Html = replaceValue(Html);
                        S.onopenobj(Html);
                    }
                } else {
                    if (R.obj != null) {
                        if (R.obj.tagName == "IMG") dialog.picAttribute(R.obj, function (html) {
                            T.setDocumentFocus();
                            T.insertHtml(html);
                        });
                    }
                }
            }
        });
    }
    var CodeInit = function () {
        HtmlBox.open();
        HtmlBox.write('<html><head><link rel="stylesheet" type="text\/css" href="skin\/css\/editcodebody.css" \/><meta http-equiv="Content-Type" content="text\/html; charset=gb2312" \/></head><body ><\/body><\/html>');
        HtmlBox.close();
        HtmlBox_body = $.Function.applyObj(HtmlBox.body);
        HtmlBox = $.Function.applyObj(HtmlBox);
        editFocus = false;
    }
    var initMode = function (value) {
        //if(T.editorMode==value)return
        switch (value) {
            case 0:
                ifr.show();
                codeBox.hide();
                DesignInit();
                break;
            case 1:
                ifr.hide();
                codeBox.show();
                codeBox.setSize(Box.offsetWidth - 6, Box.offsetHeight - 6);
                CodeInit()
                break;
            case 2:
                break;
        }
        T.editorMode = value;
        //alert(T.editorMode);
    }
    var replaceLabel = function (value) {
        var RegexEntries = [
	        /<%[\s\S]*?%>/gi,
	        /(<|&lt;)!-- #Label#[\s\S]*?--(>|&gt;)/gi,
	        /(<|&lt;)!-- #ClassLabel#[\s\S]*?--(>|&gt;)/gi,
	        /(<|&lt;)!-- #PageBar#[\s\S]*?--(>|&gt;)/gi,
	        /<!-- #SqlLabel#[\s\S]*?-->/gi,
	        /<!-- #SearchLabel#[\s\S]*?-->/gi,
	        /<object[\s\S]*?<\/object>/gi,
	        /<!-- PageSpacer -->/gi,
	        /<script[\s\S]*?<\/script>/gi,
	        /(<[^>]*|){[\s\S]*?ViewID=[\s\S]*?}/gi
	        ];
        var Img = ['AspIco', 'LabelIco', 'ClassLabelIco', 'PageBarIco', 'SqlIco', 'SearchIco', 'ObjectIco', 'PageSpacerIco', 'ScriptIco', 'ViewIco'];
        var _replace = function (protectedSource) {
            var tag = true;
            if (i == 9) {
                var m = protectedSource.match(new RegExp(/^<[^>]*/gi));
                tag = (m == null);
            }
            if (tag) {
                var Index = Labels.length;
                Labels[Index] = { html: protectedSource, index: i };
                if (Img[i] == "ViewIco") {
                    var getValue = function () {
                        var r, re; // 声明变量。
                        re = new RegExp(/ViewName=([^ ]*)( |})/gi);
                        r = protectedSource.match(re); // 在字符串 s 中查找匹配。
                        if (r != null) {
                            r[0] = r[0].replace("ViewName=", "");
                            r[0] = r[0].replace("}", "");
                            return (r[0]);
                        }
                        return ("");
                    }
                    return ('<input id="SystemPic_' + Index + '" class="ViewBox" type=button value="' + getValue() + '" />');
                } else {
                    return ('<img id="SystemPic_' + Index + '" src="\/manage\/skin\/icon\/blank.gif" class="Label ' + Img[i] + '"\/>');
                }
            } else {
                return (protectedSource);
            }
        }
        for (var i = 0; i < RegexEntries.length; i++) {
            value = value.replace(RegexEntries[i], _replace);
        }
        return (value);
    };
    var replaceValue = function (value) {
        var _replace = function (m, a, b, c, index) {
            return (Labels[index].html);
        }
        value = value.replace(/(<|&lt;)(IMG|INPUT)([^>]*)SystemPic_(\d+)([^>]*)(>|&gt;)/gi, _replace);
        value = value.replace(/©/, "&copy;");
        return (value);
    };
    var getPos = function (obj) {
        var V = {};
        obj.focus();
        var workRange = document.selection.createRange();
        V.length = workRange.text.replace(/\r?\n/g, '_').length;
        obj.select();
        var allRange = document.selection.createRange();
        workRange.setEndPoint("StartToStart", allRange);
        var len = workRange.text.replace(/\r?\n/g, '_').length;
        workRange.collapse(false);
        workRange.select();
        V.index = len - V.length;
        return V;
    };
    var setCaret = function (textbox, pos) {
        if (pos != null) {
            var r = textbox.createTextRange();
            r.collapse(true);
            r.moveStart('character', pos.index);
            r.moveEnd('character', pos.length);
            r.select();
        }
    };
    this.getDocumentFocus = function () {
        DocumentFocus = null; //,DocumentTextFocus=null
        DocumentCodeFocus = null;
        //if(editFocus){
        if (T.editorMode == 0) {
            if (ieTag) {
                Selection = HtmlBox.selection;
                DocumentFocus = HtmlBox.selection.createRange();
            }
        } else {
            if (ieTag) {
                DocumentCodeFocus = getPos(codeBox);
            } else {
                //alert(codeBox.selection);

                //var R=codeBox.getSelection();
                //if(R.getRangeAt.length>0){
                //    DocumentCodeFocus=R.getRangeAt(0);
                //    alert(1);
                //}
            }
        }
        //}
    }
    this.setDocumentFocus = function () {
        if (T.editorMode == 0) {
            if (DocumentFocus != null) {
                range = DocumentFocus; range.select(); DocumentFocus = null;
            } else { }
            HtmlBox.body.focus();
        } else {
            if (ieTag) {
                setCaret(codeBox, DocumentCodeFocus);
            }
        }

    }
    var selectMode = function (value) {
        ToolBar.disabled(value == 1);
        var selHtml = "";
        try {
            T.value = T.getValue();
        } catch (x) { }
        var Html = T.value;
        if (T.editorMode == 1 && value == 0) {
            Html = getHtmlCoding(Html);
        }
        if (T.editorMode == 0 && value == 1) {
            Html = replaceValue(Html);
            selHtml = getObjectHtml();
        }
        initMode(value);
        switch (value) {
            case 0:
                HtmlBox.body.innerHTML = Html;

                var C = Labels.length;
                /*for(var i=0;i<C;i++){
                //alert(Labels[i].html);
                var viewid=
                }*/
                //alert();
                //DesignInit();
                break;
            case 1:
                Labels.length = 0;
                codeBox.value = Html;
                break;
            case 2:
                break;
        }

    };
    T.setFocus = function () {
        if (T.editorMode == 0) {
            HtmlBox.body.focus();
        } else if (T.editorMode == 1) {
            codeBox.focus();
        }
    };
    T.selectMode = function (M) {
        ButtonCheck.selectItem(ButtonCheck.items[M]);
    };
    T.remove = function () { A.remove(); A = null; ifr = null; Box = null; End = null; codeBox = null; };

    var formatHtml = function () {
        if (T.editorMode != 0) return;
        var processFormatText = function (textContext, bz) {
            var text = DBC2SBC(textContext);
            //var text = cleanAndPaste(textContext);
            var prefix = "";
            var tmps = text.split("\n");
            var html = "";
            for (i = 0; i < tmps.length; i++) {
                var tmp = tmps[i];
                if (tmp.length > 0) {
                    if (bz)
                        if (tmp != "\r") html += "<p>" + tmp + "</p>\n";
                        else
                            if (tmp != "\r") html += "<p>" + tmp + "</p>\n";
                }
            }
            return html;
        }
        DBC2SBC = function (str) {
            var result = '';
            for (var i = 0; i < str.length; i++) {
                code = str.charCodeAt(i);
                if (code >= 65281 && code < 65373 && code != 65292 && code != 65306) {
                    result += String.fromCharCode(str.charCodeAt(i) - 65248);
                } else {
                    result += str.charAt(i);
                }
            }
            return result;
        }
        var temps = new Array();

        var html = HtmlBox.body.innerHTML;
        html = replaceUrls(html);
        var replaceImg = function (value) {
            var _replace = function (m, opener, ide, index) {
                temps[temps.length] = m;
                return ("#FormatImgID_" + (temps.length - 1) + "#");
            }
            value = value.replace(/(<|&lt;)IMG([^>]*)(>|&gt;)/gi, _replace);
            return (value);
        }
        html = replaceImg(html);
        html = html.replace(/<script[\s\S]*?<\/script>/gi, '');
        html = html.replace(/<(p|li|tr)([^>]*)>/gim, '\n');
        html = html.replace(/<\/(p|li|tr)>/gim, '\n');
        html = html.replace(/　/g, "");
        html = html.replace(/<.+?>/gim, '');
        html = processFormatText(html, true);
        if (temps != null && temps.length > 0) {
            for (j = 0; j < temps.length; j++) {
                var imghtml = "<center>" + temps[j] + "</center>";
                html = html.replace("#FormatImgID_" + j + "#", imghtml);
            }
        }
        T.setValue(html);
        return (html);
    };
    var verification = function () {
        var value = T.getValue().replace(/<.+?>/gim, '\n');
        var V = value.verification({ maxLen: S.maxLen, minLen: S.minLen, type: S.vtype, minValue: S.minValue, maxValue: S.maxValue });
        if (!V.R) {
            T.err = true;
            if (S.errMsg != null && S.errMsg != "") { T.errMsg = S.errMsg } else { T.errMsg = V.M; }
            ShowMsg(T.errMsg);
        }
        else {
            T.err = false;
            T.errMsg = "";
        }

    };
    T.check = function () { verification(); };
    var Msg = null;
    var ShowMsg = function (text) {
        if (Msg == null) {
            Msg = $B.addControl({
                xtype: 'Tip',
                HTML: text,
                className: 'M4_TipErrBox'
            });
            Msg.setXY(A.getXY().left + 30, A.getXY().top + 30);
        }
        var hide = function () {
            if (Msg != null) {
                Msg.remove();
                Msg = null;
            }
        };
        setTimeout(hide, 1500);

    };
    var HideMsg = function () {

    };
    ButtonCheck.setAttribute("onchange", function () { selectMode(ButtonCheck.getValue()); });
    if (S.value != null) T.setValue(S.value);
    else { selectMode(T.editorMode); }
    T.resize();
    T.setFocus();
}
