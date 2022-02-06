from PyQt5.QtWidgets import *

# 节点的 content， 表现
from PyQt5.QtGui import *

class QDMNodeContentWidget(QWidget):
    def __init__(self, parent = None):
        super().__init__(parent)

        self.initUI()
    
    def initUI(self):
        self.layout = QVBoxLayout()
        self.layout.setContentsMargins(0,0,0,0)
        self.setLayout(self.layout)

        self._wdg_font = QFont("Consolas",10)
        self.wdg_label = QLabel("Node Content")
        self.wdg_label.setFont(self._wdg_font)
        self.layout.addWidget(self.wdg_label)

        self.wdg_node_desc = QTextEdit("foo")
        self.wdg_node_desc.setFont(self._wdg_font)
        self.layout.addWidget(self.wdg_node_desc)
        