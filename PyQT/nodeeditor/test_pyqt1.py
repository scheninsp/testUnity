# -*- coding:utf-8 -*-  
# 启动入口
import sys
from PyQt5.QtWidgets import *
# from nodeeditor.node_editor_window import NodeEditorWindow
from node_editor_wnd import NodeEditorWnd
if __name__ == '__main__':
    app = QApplication(sys.argv)
    wnd = NodeEditorWnd()
    sys.exit(app.exec_())