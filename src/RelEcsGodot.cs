using Godot;
using Godot.Collections;

namespace RelEcs.Godot
{
    public struct NodeEntity { }

    // wraps a godot node into an ecs component
    public struct Node<T> : IReset<Node<T>> where T : Node
    {
        public T Value;
        public Node(T value) => Value = value;
        
        public void Reset(ref Node<T> c)
        {
            c.Value?.QueueFree();
            c.Value = null;
        }
    }

    // wraps an ecs object into a godot variant
    public class Marshallable<T> : Reference
    {
        public T Value;
        public Marshallable(T value) => Value = value;
    }

    public static class CommandsExtensions
    {
        public static Entity Spawn(this Commands commands, Node parent)
        {
            var entity = commands.Spawn().Add<IsA, NodeEntity>();

            var nodes = new Array();
            nodes.Add(parent);

            foreach (Node child in parent.GetChildren())
            {
                nodes.Add(child);
            }

            foreach (Node node in nodes)
            {
                var addMethod = typeof(CommandsExtensions).GetMethod("AddNodeHandle");
                var addChildMethod = addMethod?.MakeGenericMethod(new[] { node.GetType() });
                addChildMethod?.Invoke(null, new object[] { entity, node });
            }

            return entity;
        }

        public static void AddNodeHandle<T>(Entity entity, T node) where T : Node
        {
            entity.Add(new Node<T>(node));
            node.SetMeta("Entity", new Marshallable<Entity>(entity));
        }
    }
}