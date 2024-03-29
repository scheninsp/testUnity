from PyQt5.QtWidgets import *
from PyQt5.QtGui import *
from PyQt5.QtCore import *

# 从属于 Socket 类，表现
class QDMGraphicsSocket(QGraphicsItem):
    def __init__(self, socket:'Socket'):
        super().__init__(socket.node.grNode)

        self.socket = socket

        self.radius = 6.0
        self.outline_width = 1.0
        self._color_background = QColor("#FFFF7700")
        self._color_outline = QColor("FF000000")

        self._pen = QPen(self._color_outline)
        self._pen.setWidthF(self.outline_width)
        self._brush = QBrush(self._color_background)

    def paint(self, painter, QStyleOptionGraphicsItem, widget=None):
        painter.setBrush(self._brush)
        painter.setPen(self._pen)
        painter.drawEllipse(-self.radius, -self.radius, 2*self.radius, 2*self.radius)

    def boundingRect(self):
        return QRectF(-self.radius - self.outline_width, -self.radius - self.outline_width, 2*self.radius * self.outline_width, 2*self.radius * self.outline_width)

    def mousePressEvent(self, event: 'QGraphicsSceneMouseEvent') -> None:
        return super().mousePressEvent(event)