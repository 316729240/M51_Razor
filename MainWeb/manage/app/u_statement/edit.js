$M.u_statement.edit = function (S) {
    var tab = mainTab.addItem({ text: "声明", "class": "form-horizontal", closeButton: true });
    tab.focus();
    var toolBar = tab.addControl({
        xtype: "ToolBar", class: "note-toolbar",
        items: [
            [{ text: "保存", ico: "fa-save", primary: true, onClick: function () { form.submit(); } }, { text: "新增", ico: "fa-plus", onClick: function () { form.reset(); } }]
        ]
    });
    var form=tab.addControl({
        xtype: "Form",
        command: "u_statement.ajax.edit",
        templateUrl: $M.config.appPath + "u_statement/edit.html",
        onBeginSubmit: function (sender) {
            var u_firstname = sender.find("u_firstName").val();
            var u_lastname = sender.find("u_lastName").val();
            sender.find("title").val(u_firstname +" "+ u_lastname);
        },
        onSubmit: function () {
            if (S.back) S.back();
            $M.alert("保存成功", tab.remove);
        },
        onLoad: function (sender) {
            sender.find("id").attr({ value: S.dataId });
            sender.find("classId").attr({ value: S.classId, defaultValue: S.classId });
            sender.read("u_statement.ajax.read", S.dataId, function (json) {
                sender.val(json);
            });
        }
    });
};
