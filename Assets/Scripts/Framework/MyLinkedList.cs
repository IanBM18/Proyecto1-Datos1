using System;
using System.Collections;
using System.Collections.Generic;

public class MyLinkedList<T> : IEnumerable<T>
{
    private class Node
    {
        public T Value;
        public Node Next;
        public Node(T v) { Value = v; Next = null; }
    }

    private Node head;
    private Node tail;
    public int Count { get; private set; }

    public MyLinkedList() { head = tail = null; Count = 0; }

    public void AddLast(T value)
    {
        var n = new Node(value);
        if (head == null) head = tail = n;
        else { tail.Next = n; tail = n; }
        Count++;
    }

    public T PopFirst()
    {
        if (head == null) throw new InvalidOperationException("Empty list");
        var v = head.Value;
        head = head.Next;
        if (head == null) tail = null;
        Count--;
        return v;
    }

    public bool Remove(T value)
    {
        Node prev = null, cur = head;
        var comparer = EqualityComparer<T>.Default;
        while (cur != null)
        {
            if (comparer.Equals(cur.Value, value))
            {
                if (prev == null) head = cur.Next;
                else prev.Next = cur.Next;
                if (cur == tail) tail = prev;
                Count--;
                return true;
            }
            prev = cur;
            cur = cur.Next;
        }
        return false;
    }

    public bool Contains(T value)
    {
        var cur = head;
        var comparer = EqualityComparer<T>.Default;
        while (cur != null)
        {
            if (comparer.Equals(cur.Value, value)) return true;
            cur = cur.Next;
        }
        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var cur = head;
        while (cur != null)
        {
            yield return cur.Value;
            cur = cur.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}