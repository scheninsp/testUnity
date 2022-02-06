from PyQt5.QtWidgets import QGraphicsView
from PyQt5.QtCore import *
from PyQt5.QtGui import *

from node_graphics_socket import QDMGraphicsSocket
from node_edge import Edge, EDGE_TYPE_BEZIER

MODE_NOOP = 1
MODE_EDGE_DRAG = 2

EDGE_DRAG_START_THRESHOLD = 10.0

# 主窗口的 View
class QDMGraphicsView(QGraphicsView):
    def __init__(self, grScene, parent=None):
        super().__init__(parent)
        self.grScene = grScene

        self.initUI()
        self.setScene(grScene)

        self.mode = MODE_NOOP

        self.zoonInFactor = 1.25
        self.zoomClamp = False  # Useless
        self.zoom = 10  
        self.zoomStep = 1
        self.zoomRange = [0, 10]  

    def initUI(self):
        self.setRenderHints(QPainter.Antialiasing | QPainter.HighQualityAntialiasing | QPainter.TextAntialiasing | QPainter.SmoothPixmapTransform)

        self.setViewportUpdateMode(QGraphicsView.FullViewportUpdate)

        self.setHorizontalScrollBarPolicy(Qt.ScrollBarAlwaysOff)
        self.setVerticalScrollBarPolicy(Qt.ScrollBarAlwaysOff)

        self.setTransformationAnchor(QGraphicsView.AnchorUnderMouse)

    def mousePressEvent(self, event):
        if event.button() == Qt.MiddleButton:
            self.middleMouseButtonPress(event)
        elif event.button() == Qt.LeftButton:
            self.leftMouseButtonPress(event) 
        elif event.button() == Qt.RightButton:
            self.rightMouseButtonPress(event)
        else:
            super().mousePressEvent(event)

    def mouseReleaseEvent(self, event):
        if event.button() == Qt.MiddleButton:
            self.middleMouseButtonRelease(event)
        elif event.button() == Qt.LeftButton:
            self.leftMouseButtonRelease(event) 
        elif event.button() == Qt.RightButton:
            self.rightMouseButtonRelease(event)
        else:
            super().mouseReleaseEvent(event)

    def middleMouseButtonPress(self, event):
        # 中键拖动面板
        # 先释放左键
        releaseEvent = QMouseEvent(QEvent.MouseButtonRelease, event.localPos(), event.screenPos(), Qt.LeftButton, Qt.NoButton, event.modifiers())
        super().mouseReleaseEvent(releaseEvent)
        self.setDragMode(QGraphicsView.ScrollHandDrag)
        fakeEvent = QMouseEvent(event.type(), event.localPos(), event.screenPos(), Qt.LeftButton, event.buttons()|Qt.LeftButton, event.modifiers())
        super().mousePressEvent(fakeEvent)

    def middleMouseButtonRelease(self, event):
        fakeEvent = QMouseEvent(event.type(), event.localPos(), event.screenPos(), Qt.LeftButton, event.buttons() & -Qt.LeftButton, event.modifiers())
        super().mouseReleaseEvent(fakeEvent)
        self.setDragMode(QGraphicsView.NoDrag)

    def edgeDragStart(self, item):
        print("Start drag edge")
        # self.dragEdge 是临时边
        self.previousEdge = item.socket.edge
        self.last_start_socket = item.socket
        self.dragEdge = Edge(self.grScene.scene, item.socket, None, EDGE_TYPE_BEZIER)

    def edgeDragEnd(self, item):
        self.mode = MODE_NOOP
        print("End drag edge")
        if type(item) is QDMGraphicsSocket:
            print("Assign End socket")
            if item.socket.hasEdge():
                self.tempEdge = item.socket.edge 
                item.socket.edge.remove()
                print(self.tempEdge != self.previousEdge)
            if self.previousEdge is not None and self.tempEdge != self.previousEdge: 
                # 可能重复连接，是同一条边，那就不要remove两次
                self.previousEdge.remove()
            self.tempEdge = None
            self.dragEdge.end_socket = item.socket
            self.dragEdge.start_socket.setConnectedEdge(self.dragEdge)
            self.dragEdge.end_socket.setConnectedEdge(self.dragEdge)
            self.dragEdge.updatePositions()
            return True

        self.dragEdge.remove()  # 清理 grEdge 和当前 edge
        self.dragEdge = None
        if self.previousEdge is not None:
            self.previousEdge.start_socket.edge = self.previousEdge
            # 在 edgeDragStart 生成新的 Edge 时修改了 start_socket.edge，需要恢复
        return False

    def distanceBetweenClickAndReleaseIsOff(self, event):
        new_lmb_release_scene_pos = self.mapToScene(event.pos()) # 用来和 MousePress 时对比判断鼠标移动距离
        dist_scene_pos = new_lmb_release_scene_pos - self.last_lmb_click_scene_pos
        dist_scene_pos_len = dist_scene_pos.x() * dist_scene_pos.x() + dist_scene_pos.y() * dist_scene_pos.y()
        return dist_scene_pos_len > EDGE_DRAG_START_THRESHOLD * EDGE_DRAG_START_THRESHOLD

    def leftMouseButtonPress(self, event):

        item = self.getItemAtClick(event)

        self.last_lmb_click_scene_pos = self.mapToScene(event.pos())

        # 单击 socket 开始生成一条新的边
        # 进入 EDGE_DRAG 模式
        if type(item) is QDMGraphicsSocket:
            if self.mode == MODE_NOOP:
                self.mode = MODE_EDGE_DRAG
                self.edgeDragStart(item)
                return
        
        # 在 EDGE_DRAG 模式下单击左键退出模式，如果终点是socket，就把边放上去
        if self.mode == MODE_EDGE_DRAG:
            if self.edgeDragEnd(item):
                return

        super().mousePressEvent(event)


    def leftMouseButtonRelease(self, event):
        item = self.getItemAtClick(event)

        # 如果在 EDGE_DRAG 模式下，抬起左键时，如果在 socket 上就生成新的连接
        if self.mode == MODE_EDGE_DRAG:
            if self.distanceBetweenClickAndReleaseIsOff(event): 
                # 只有鼠标移出一段距离才会assign socket，防止连到自己
                print("ReleaseOff")
                if self.edgeDragEnd(item):
                    return
            print("No ReleaseOff")

        super().mouseReleaseEvent(event)

    def rightMouseButtonPress(self, event):
        super().mousePressEvent(event)
        
    def rightMouseButtonRelease(self, event):
        super().mouseReleaseEvent(event)

    def mouseMoveEvent(self, event):
        if self.mode == MODE_EDGE_DRAG:
            pos = self.mapToScene(event.pos())
            self.dragEdge.grEdge.setDestination(pos.x(), pos.y())
            self.dragEdge.grEdge.update()  # redraw
            
        super().mouseMoveEvent(event)

    def wheelEvent(self, event):
        # 滚轮缩放整体面板
        zoomOutFactor = 1./self.zoonInFactor
        oldPos = self.mapToScene(event.pos())
        if event.angleDelta().y() > 0:
            zoomFactor = self.zoonInFactor
            self.zoom += self.zoomStep
        else: 
            zoomFactor = zoomOutFactor
            self.zoom -= self.zoomStep

        if self.zoom < self.zoomRange[0]: 
            self.zoom = self.zoomRange[0]
            zoomFactor = 1.0
        elif self.zoom > self.zoomRange[1]:
            self.zoom = self.zoomRange[1]
            zoomFactor = 1.0

        self.scale(zoomFactor, zoomFactor)

    def getItemAtClick(self,event):
        pos = event.pos()
        obj = self.itemAt(pos)
        return obj