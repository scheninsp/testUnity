
# 节点之间的连线
from email.errors import InvalidBase64CharactersDefect
from node_graphics_edge import *

EDGE_TYPE_DIRECT = 1
EDGE_TYPE_BEZIER = 2

class Edge:
    def __init__(self, scene, start_socket, end_socket, type=EDGE_TYPE_DIRECT):

        self.scene = scene
        self.start_socket = start_socket
        self.end_socket = end_socket

        # 把这个 edge 注册到它的 sockets 里
        self.start_socket.edge = self
        if self.end_socket is not None:
            self.end_socket.edge = self

        self.grEdge = QDMGraphicsEdgeDirect(self) if type == EDGE_TYPE_DIRECT else QDMGraphicsEdgeBezier(self)

        self.updatePositions()
        self.scene.grScene.addItem(self.grEdge)
        self.scene.addEdge(self)

    # 从注册到自身的两个 socket 之间画一条连线
    def updatePositions(self):
        source_pos = self.start_socket.getSocketPosition()
        source_pos[0] +=  self.start_socket.node.grNode.pos().x()
        source_pos[1] +=  self.start_socket.node.grNode.pos().y()

        self.grEdge.setSource(*source_pos)
        if self.end_socket is not None:
            end_pos = self.end_socket.getSocketPosition()
            end_pos[0] +=  self.end_socket.node.grNode.pos().x()
            end_pos[1] +=  self.end_socket.node.grNode.pos().y()
            self.grEdge.setDestination(*end_pos)
        else:
            self.grEdge.setDestination(*source_pos)
        self.grEdge.update()
        

    def remove_from_sockets(self):
        if self.start_socket is not None:
            self.start_socket.edge = None
        if self.end_socket is not None:
            self.end_socket.edge = None

        self.start_socket = None
        self.end_socket = None

    def remove(self):
        self.remove_from_sockets()
        self.scene.grScene.removeItem(self.grEdge)
        self.grEdge = None
        self.scene.removeEdge(self)           
