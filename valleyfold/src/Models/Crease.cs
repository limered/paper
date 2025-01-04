namespace valleyfold.Models;

public struct Crease
{
    /**
     *     //type = 0 panel, 1 crease

    //face1 corresponds to node1, face2 to node2
    this.edge = edge;
    for (var i=0;i<edge.nodes.length;i++){
        edge.nodes[i].addInvCrease(this);
    }
    this.face1Index = face1Index;//todo this is useless
    this.face2Index = face2Index;
    this.targetTheta = targetTheta;
    this.type = type;
    this.node1 = node1;//node at vertex of face 1
    this.node2 = node2;//node at vertex of face 2
    this.index = index;
    node1.addCrease(this);
    node2.addCrease(this);
     */

    private uint Index;
    
    private uint face1Index;
    private uint face2Index;

    private float targetTheta;

    private ModelNode Node1;
    private ModelNode Node2;

}