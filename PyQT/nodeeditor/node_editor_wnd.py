from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtGui import *
# from nodeeditor.node_editor_window import NodeEditorWindow
from node_graphics_scene import QDMGraphicsScene
from node_graphics_view import QDMGraphicsView
from node_scene import Scene
from node_node import Node
from node_edge import Edge

# 主窗口
class NodeEditorWnd(QWidget):
    def __init__(self, parent = None):
        super().__init__(parent)
        self.initUI()

    def initUI(self):
        self.setGeometry(200,200,800,800)
        self.layout = QVBoxLayout()
        self.layout.setContentsMargins(0,0,0,0)
        self.setLayout(self.layout)

        self.scene = Scene()
        self.grScene = self.scene.grScene

        self.view = QDMGraphicsView(self.grScene, self)
        self.layout.addWidget(self.view)

        node1 = Node(self.scene, "First Node", inputs=[1,1,1], outputs=[1])  # 数字代表 socket 类型， socket index 按数组顺序
        node2 = Node(self.scene, "Second Node", inputs=[1,1,1], outputs=[1])  
        node3 = Node(self.scene, "Third Node", inputs=[1,1,1], outputs=[1]) 
        node1.setPos(-350,100)
        node2.setPos(-75,0)
        node3.setPos(200,-100)

        edge1 = Edge(self.scene, node1.outputs[0], node2.inputs[0])
        edge2 = Edge(self.scene, node2.outputs[0], node3.inputs[1], 2)

        self.setWindowTitle("Node Editor")
        self.show()
        # self.addDebugComponent()

    def addDebugComponent(self):
        # a green box
        greenBrush = QBrush(Qt.green)
        outlinePen = QPen(Qt.black)
        outlinePen.setWidth(2)
        rect = self.grScene.addRect(-100,-100,80,100, outlinePen, greenBrush)
        rect.setFlag(QGraphicsItem.ItemIsMovable)