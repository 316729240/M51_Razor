$M.system.displayField = function (S) {
    var win = new $M.dialog({
        title: "设置显示字段",
        ico: "fa-list-alt",
        style: { width: "400px", height: "480px" },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                var fields = "";
                for (var i = 0; i < grid.selectedRows.length; i++) {
                    if (i > 0) fields += ",";
                    fields += grid.selectedRows[i].cells[0].val();
                }
                $M.comm("saveDisplayField", { id: S.id, fields: fields }, function () {
                    if (S.back) S.back();
                });
            }
        }
    });
    win.show();
    var grid = win.addControl({ xtype: "GridView", style: { height: "300px" },
        allowMultiple: true,
        columns: [{ text: "name", name: "name", visible: false }, { text: "字段", name: "text", width: 260}]
    });
    $M.comm("fieldInfo", { id: S.id }, function (json) {
        var index = 0;
        for (var i = 0; i < json.length; i++) {
            if (json[i].type != "Remark") {
                grid.addRow([json[i].name, json[i].text]);
                if (json[i].visible) grid.rows[i].focus();
                index++;
            }

        }
    });
};
$M.system.dataManage = function (S) {
    var text = S.text, moduleId = S.moduleId, classId = S.classId, back = S.back;
    var extensionMenu = null;
    var _p = ["moduleId", "dataTypeId"];
    if (S.classId) _p = ["classId", "dataTypeId"];
    extensionMenu = $M.loadInterface(_p, S.dataTypeId, function (name) {
        $M.app.call(name, S);
    });
    var item = null;
    if ($.dataManage) {
        $.dataManage._moduleId = moduleId;
        $.dataManage._classId = classId;
        $.dataManage.attr("text", text);
    } else {
        $.dataManage = mainTab.addItem({ text: text, closeButton: true, onClose: function () { $.dataManage = null; } });
        $.dataManage._moduleId = moduleId;
        $.dataManage._classId = classId;
        item = $.dataManage;

        var searchMenu = $(document.body).addControl({ xtype: "Menu", onItemClicked:
            function (sender, e) {
                item._searchField = e.attr("value");
                searchF.val("搜索'" + e.attr("text") + "'");
            }, items: [{ text: "id", value: "id" }, { text: "标题", value: "title" }, { text: "发布人", value: "userId"}]
        });
        var attrIndex = 0;
        var toolBar = item.addControl({
            xtype: "ToolBar",
            items: [
                {
                    xtype: "ButtonCheckGroup", items: [{ text: "已审核", value: 0, ico: "fa-check-square-o" }, { text: "未审核", value: 1, ico: "fa-exclamation-circle" }, { text: "回收站", value: 2, ico: "fa-recycle"}], value: 0,
                    onClick: function (sender, e) {
                        loadPage(1, item._orderByName, item._sortDirection, e.value);
                    }
                },
                [{ xtype: "InputGroup", name: "searchGroup", style: { width: "300px" }, items: [
                    { xtype: "Button", text: "搜索'标题'", menu: searchMenu, name: "searchF" },
                    { xtype: "TextBox", text: "", name: "searchInput" },
                    { xtype: "Button", ico: "fa-search", name: "searchButton"}]
                }],
                [
                    { xtype: "Button", text: "添加", name: "appendButton", ico: "fa-plus", onClick: function () { addData(); } },
                    { xtype: "Button", text: "修改", name: "editButton", ico: "fa-edit", onClick: function () { editData(); } },
                    { xtype: "Button", text: "删除", name: "delButton", ico: "fa-trash-o", onClick: function () { delData(); } },
                    { xtype: "Button", text: "移动", name: "moveButton", ico: "fa-arrows", onClick: function () { moveData(); } }
                ],
                [{ xtype: "Button", text: "属性设置", name: "attrSetButton", ico: " fa-cog", menu: [
		            { text: "审核" },
		            { text: "置顶", checked: true },
		            { text: "-" },
		            { text: "自定义属性", items: [{ text: "推荐" }, { text: "精华" }, { text: "热点"}] }
                ]
                }],
                [{ text: "扩展工具", ico: "fa-share-alt", menu: extensionMenu}]
            ]
        });
        var searchGroup = toolBar.find("searchGroup");
        var searchF = searchGroup.find("searchF");
        var searchButton = searchGroup.find("searchButton");
        var searchInput = searchGroup.find("searchInput");
        var attrSetButton = toolBar.find("attrSetButton");
        var editButton = toolBar.find("editButton");
        var delButton = toolBar.find("delButton");
        var moveButton = toolBar.find("moveButton");
        searchInput.attr("onKeyDown", function (sender, e) {
            if (e.which == 13) {
                item._keyword = searchInput.val();
                loadPage(1, item._orderByName, item._sortDirection, item._type);
            }
        });
        searchButton.attr("onClick", function () {
            item._keyword = searchInput.val();
            loadPage(1, item._orderByName, item._sortDirection, item._type);
        });
        var setEditButtonStatus = function (flag) {
            attrSetButton.enabled(flag);
            editButton.enabled(flag);
            delButton.enabled(flag);
            moveButton.enabled(flag);
        };
        setEditButtonStatus(false);
        item._pageNo = 1;
        item._orderByName = null;
        item._sortDirection = null;
        item._type = 0;
        item._searchField = "title";
        item._keyword = "";
        var grid = null;
        var reload = function () {
            loadPage(item._pageNo, item._orderByName, item._sortDirection, item._type);
        };
        var getId = function () {
            var ids = "";
            for (var i = 0; i < grid.selectedRows.length; i++) {
                if (i > 0) ids += ",";
                ids += grid.selectedRows[i].cells[0].val();
            }
            return ids;
        };
        var moveData = function () {
            if (grid.selectedRows.length == 0) return;
            $M.dialog.selectColumn(S.moduleId, function (json) {
                if (S.classId == json.id) { $M.alert("目标栏目不能为当前栏目"); return; }
                $M.comm("moveData", { ids: getId(), classId: json.id }, function () {
                    reload();
                });
            });
        };
        var setField = function () {
            $M.app.call("$M.system.displayField", { id: S.dataTypeId, back: function () { reload(); } });
        };
        var setTop = function (sender, item) {
            if (grid.selectedRows.length == 0) return;
            var ids = getId();
            $M.comm("setTop", { ids: ids, flag: (item.attr("checked") ? 0 : 1) }, function (json) {
                reload();
            });
        };
        var delData = function () {
            if (grid.selectedRows.length == 0) return;
            $M.confirm("您确定要删除所选数据吗？", function () {
                var ids = getId();
                if (ids != "") {
                    $M.comm("delData", { moduleId: item._moduleId, classId: item._classId, ids: ids, tag: (item._type == 0 ? 0 : 1) }, function () {
                        reload();
                    });
                }
            });
        };
        var addData = function () {

            $M.app.openEdit(22192428132, { dataId: null, classId: item._classId });
        };
        var editData = function () {
            if (grid.selectedRows.length == 0) return;
            $M.app.openEdit(22192428132, { dataId: grid.selectedRows[0].cells[0].val() });
        };
        var setAttr = function (sender, item) {
            if (grid.selectedRows.length == 0) return;
            var ids = getId();
            $M.comm("setAttr", { ids: ids, type: item.attr("value"), flag: (item.attr("checked") ? 0 : 1) }, function (json) {
                reload();
            });
        };
        var menu1 = $(document.body).addControl({ xtype: "Menu", items: [
            { text: "编辑", onClick: editData },
            { text: "-" },
            { text: "至顶", onClick: setTop },
            { text: "自定义属性", items: [
                { text: "推荐", value: "0", onClick: setAttr }, { text: "精华", value: "1", onClick: setAttr }, { text: "热点", value: "2", onClick: setAttr }
            ]
            },
            { text: "-" },
		    { text: "移动", onClick: moveData },
		    { text: "删除", onClick: delData },
            { text: "-" },
		    { text: "设置", onClick: setField },
		    { text: "属性" }
        ]
        });
        grid = item.addControl({
            xtype: "GridView",
            dock: $M.Control.Constant.dockStyle.fill,
            allowMultiple: true,
            allowSorting: true,
            contextMenuStrip: menu1,
            onSorting: function (sender, e) {
                loadPage(1, e.name, e.sortDirection, item._type);
            },
            onKeyDown: function (sender, e) {
                if (e.which == 46) delData();
            },
            onRowCommand: function (sender, e) {
                if (e.commandName == "link") {
                    $M.app.openEdit(22192428132, { classId: S.classId, dataId: sender.rows[e.y].cells[0].val() });
                }
            },
            onMouseDown: function (sender, e) {
                if (e.which == 3) {
                    var flag = [0, 0, 0, 0, 0, 0];
                    for (var i = 0; i < grid.selectedRows.length; i++) {
                        attr = grid.selectedRows[i].cells[attrIndex].val().split(",");
                        for (var i1 = 0; i1 < 6; i1++) {
                            flag[i1] = (flag[i1] | parseInt(attr[i1]));
                        }
                    }
                    menu1.items[2].attr("checked", (flag[0] == 1));
                    for (var i = 0; i < menu1.items[3].items.length; i++) {
                        menu1.items[3].items[i].attr("checked", (flag[i + 1] == 1));
                    }
                }
            },
            onCellFormatting: function (sender, e) {
                for (var i = 0; i < dateField.length; i++) {
                    if (e.columnIndex == dateField[i]) {
                        var value = new Date(parseInt(e.value.replace(/\D/igm, "")));
                        return value.format("yyyy-MM-dd hh:mm:ss");
                    }
                }
                return e.value;
            },
            onSelectionChanged: function (sender, e) {
                setEditButtonStatus(sender.selectedRows.length > 0);
            }
        });
        var dateField = null;
        var loadPage = function (pageNo, orderByName, sortDirection, type) {
            item._orderByName = orderByName == null ? "" : orderByName;
            item._type = type;
            item._sortDirection = sortDirection == null ? 0 : sortDirection;
            $M.comm("dataList", {
                moduleId: item._moduleId,
                classId: item._classId,
                pageNo: pageNo,
                orderBy: item._orderByName,
                sortDirection: item._sortDirection,
                type: item._type,
                searchField: item._searchField,
                keyword: item._keyword
            },
            function (json) {

                grid.clear();
                //if (!grid.attr("columns")) {
                dateField = [];
                json[0][0].isLink = true;
                for (var i = 0; i < json[0].length; i++) {
                    json[0][i].isLink = json[0][i].isTitle;
                    if (json[0][i].name == "attribute") attrIndex = i;
                    if (json[0][i].type == "Date") dateField[dateField.length] = i;
                    if (json[0][i].name == item._orderByName) json[0][i].sortDirection = item._sortDirection;
                }
                grid.attr("columns", json[0]);
                //}
                //item._classId = classId;
                grid.addRow(json[1].data);
                item.pageBar.attr("pageSize", json[1].pageSize);
                item.pageBar.attr("recordCount", json[1].recordCount);
                item.pageBar.attr("pageNo", json[1].pageNo);
                item.resize();
                setEditButtonStatus(false);
            });
        };
        item.pageBar = item.addControl({
            xtype: "PageBar",
            onPageChanged: function (sender, e) {
                item._pageNo = e.pageNo;
                loadPage(item._pageNo, item._orderByName, item._sortDirection, item._type);
            }
        });

    }
    $.dataManage.pageBar.go(1);
    $.dataManage.focus();
};