from node_graphics_scene import QDMGraphicsScene

# 窗口的 scene
class Scene:
    def __init__(self):
        self.nodes = []
        self.edges = []

        self.scene_width = 64000
        self.scene_height = 64000

        self.initUI()

    def initUI(self):
        self.grScene = QDMGraphicsScene()
        self.grScene.setGrScene(self.scene_width, self.scene_height)

    def addNode(self, node):
        self.nodes.append(node)
        self.grScene.addItem(node.grNode)
    
    def removeNode(self, node):
        self.grScene.remove(node.grNode)
        self.nodes.remove(node)

    def addEdge(self, edge):
        self.edges.append(edge)

    def removeEdge(self, edge):
        self.edges.remove(edge)