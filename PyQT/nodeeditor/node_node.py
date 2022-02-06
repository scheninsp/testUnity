from node_graphics_node import QDMGraphicsNode
from node_content_widget import QDMNodeContentWidget
from node_socket import *

# 节点
class Node:
    def __init__(self, scene, title="Undefined", inputs=[], outputs=[]):
        self.scene = scene
        self.title = title

        self.content = QDMNodeContentWidget()

        self.grNode = QDMGraphicsNode(self, self.title)

        self.scene.addNode(self)

        # Socket container
        self.inputs = []
        self.outputs = []

        counter = 0
        for item in inputs:
            socket = Socket(self, counter, LEFT_TOP)
            self.inputs.append(socket)
            counter += 1

        counter = 0
        for item in outputs:
            socket = Socket(self, counter, RIGHT_TOP)
            self.outputs.append(socket)
            counter += 1

    @property
    def pos(self):
        return self.grNode.pos()

    def setPos(self, x, y):
        self.grNode.setPos(x,y)


    def getSocketPosition(self, index, position):
        if position in (LEFT_TOP, LEFT_BOTTOM):
            x = 0
        else:
            x = self.grNode.width

        y = self.grNode.title_height + self.grNode._padding + self.grNode.edge_size + index * 30
        return [x,y]


    def updateConnectedEdges(self):
        for socket in self.inputs + self.outputs:
            if socket.hasEdge():
                socket.edge.updatePositions()