from node_graphics_socket import QDMGraphicsSocket

LEFT_TOP = 1
LEFT_BOTTOM = 2
RIGHT_TOP = 3
RIGHT_BOTTOM = 4

# 节点的输入输出挂接点
class Socket():
    def __init__(self, node, index = 0, position=LEFT_TOP):

        self.node = node
        self.index = index
        self.position = position

        self.grSocket = QDMGraphicsSocket(self)

        self.grSocket.setPos(*self.node.getSocketPosition(index, position))

        # Edge Container
        self.edge = None

    def getSocketPosition(self):
        return self.node.getSocketPosition(self.index, self.position)
    
    def setConnectedEdge(self, edge=None):
        self.edge = edge

    def hasEdge(self):
        return self.edge is not None