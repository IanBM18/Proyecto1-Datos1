using System;
using System.Collections;
using System.Collections.Generic;

public class MyLinkedList<T> : IEnumerable<T>
{
    private class Node { public T Value; public Node Next; public Node(T v) { Value = v; Next = null; } }
    private Node head, tail;
    public int Count { get; private set; }
    public MyLinkedList() { head = tail = null; Count = 0; }
    public void AddLast(T v)
    {
        var n = new Node(v);
        if (head == null) head = tail = n;
        else { tail.Next = n; tail = n; }
        Count++;
    }
    public T PopFirst()
    {
        if (head == null) throw new InvalidOperationException("Empty");
        var v = head.Value; head = head.Next; if (head == null) tail = null; Count--; return v;
    }
    public bool Contains(T v)
    {
        var cur = head;
        var cmp = EqualityComparer<T>.Default;
        while (cur != null) { if (cmp.Equals(cur.Value, v)) return true; cur = cur.Next; }
        return false;
    }
    public IEnumerator<T> GetEnumerator() { var cur = head; while (cur != null) { yield return cur.Value; cur = cur.Next; } }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
