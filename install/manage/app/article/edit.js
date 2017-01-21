$M.article = {
    //后台数据编辑入口函数 命名为 edit 不能更改
    edit: function (S) {
        var tab = mainTab.addItem({ text: "文章编辑", "class": "form-horizontal", closeButton: true });

        var toolBar = tab.addControl({
            xtype: "ToolBar", class: "note-toolbar",
            items: [
                [{ text: "保存", ico: "fa-save", color: 2, onClick: function () { form.submit(); } }, { text: "打开网址", ico: "fa-external-link", onClick: function () {
                    if (S.url) window.open(S.url);
                }
                }],
                [{ text: "选择模板",
                    onClick: function () {
                        $M.dialog.selectTemplate({ classId: S.classId, skinId: S.skinId,
                            back: function (id) {
                                skinId.val(id);
                            }
                        });
                    }
                }]
                ]
        });
        var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: "*" }, { size: 300, visible: false}] });
        //frame.attr("items", [{ size: "*" }, { size: 0}]);
        //frame.items[1].attr("size",0);
        frame.items[0].css({ "overflow-y": "auto", "overflow-x": "hidden" });
        tab.focus();
        var form = frame.items[0].addControl({
            xtype: "Form",
            command: "article.ajax.edit",
            onBeginSubmit: function () {
                var xml = new $M.xml();
                var domRoot = xml.addDom("variables");
                var data = grid.val();
                for (var i = 0; i < data.length; i++) {
                    var node = domRoot.addDom("item")
                    node.val(data[i][1]);
                    node.attr("name", data[i][0]);
                }
                custom.val(xml.getXML());
            },
            onSubmit: function (sender, e) {
                $M.alert("保存成功！");
                read(e.returnData);
            }
        });
        var custom = form.append("<textarea  name=\"u_custom\" style=\"display:none\"></textarea>");
        var skinId = form.append("<input name=skinId value='' type='hidden'>");
        form.append("<input name=id value='" + S.dataId + "' type='hidden'>");
        form.append("<input name=classId value='" + S.classId + "' type='hidden'>");
        form.addControl([
            { xtype: "TextBox", name: "title", labelText: "文章标题", labelWidth: 2 },
            { xtype: "TextBox", name: "u_keyword", labelText: "关 键 词", labelWidth: 2 },
            { xtype: "UploadFileBox", name: "u_defaultPic", labelText: "缩 略 图", labelWidth: 2 },
            { xtype: "TextBox", name: "u_info", labelText: "文章摘要", labelWidth: 2, multiLine: true, style: { height: "50px"} },
            { xtype: "Editor", name: "u_content", style: { height: 300 }, labelText: "文章内容", labelWidth: 2 }
        ]);
        var grid = frame.items[1].addControl({
            xtype: "GridView", dock: 2,
            columns: [
                { text: "名称", name: "name", width: 60 },
                { text: "值", xtype: "TextBox", name: "value", width: 200 }
            ]
        });
        var read = function (dataId) {
            if (dataId) {
                $M.comm("article.ajax.read", { id: dataId }, function (json) {
                    setForm(json);
                });
            }
        };
        var setForm = function (data) {
            form.val(data);
            S.skinId = data.skinId;
            S.url = data.url;
            var xml = new $M.xml(data.u_custom);
            for (var i = 0; i < grid.rows.length; i++) {
                if (xml.documentElement.childNodes && xml.documentElement.childNodes.length > i) grid.rows[i].cells[1].val(xml.documentElement.childNodes[i].text);
            }
        };
        var comm = [["getCustomField", { classId: S.classId}]];
        if (S.dataId != null) comm[comm.length] = ["article.ajax.read", { id: S.dataId}];
        $M.comm(comm, function (data) {
            if (data[0].variables) {
                grid.addRow(data[0].variables.item);
                frame.items[1].attr("visible", true);
            }
            if (S.dataId != null) setForm(data[1]);
        });
    }
};