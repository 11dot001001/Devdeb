using Devdeb.Sets.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Devdeb.Sets.Generic
{
	public class RedBlackTreeSurjection<TInput, TOutput>
	{
		private struct Slot
		{
			public TInput Input;
			public TOutput Output;
			public bool IsRed;
			public int Left;
			public int Right;

			public Slot
			(
				TInput input,
				TOutput output,
				bool isRed,
				int left = -1,
				int right = -1
			)
			{
				Input = input;
				Output = output;
				IsRed = isRed;
				Left = left;
				Right = right;
			}

			public bool IsBlack => !IsRed;
		}
		private enum RotationDirection
		{
			Right = 0,
			LeftRight = 1,
			RightLeft = 2,
			Left = 3,
		}

		private const int DefaultCapacity = 4;

		private readonly IComparer<TInput> _inputComparer;
		private Slot[] _slots;
		private int _rootIndex;
		private int _freeListIndex;
		private int _usedSlotsCount;
		private int _count;

		public RedBlackTreeSurjection(IComparer<TInput> inputComparer = null, int capacity = DefaultCapacity)
		{
			_inputComparer = inputComparer ?? Comparer<TInput>.Default;
			_slots = new Slot[capacity];
			_rootIndex = -1;
			_freeListIndex = -1;
		}

		public int Count => _count;

		public void Add(TInput input, TOutput output)
		{
			bool result = TryAdd(input, output);
			if (!result)
				throw new ArgumentException($"An {nameof(input)} element with the same vaue {input} already exists.");
		}
		public bool TryAdd(TInput input, TOutput output)
		{
			if (_rootIndex == -1)
			{
				_rootIndex = TakeSlot();
				_slots[_rootIndex] = new Slot(input, output, false);
				return true;
			}

			int current = _rootIndex;
			int parent = -1;
			int grandParent = -1;
			int greatGrandParent = -1;
			try
			{
				for (; ; )
				{
					Slot slot = _slots[current];
					int order = _inputComparer.Compare(input, slot.Input);
					if (order == 0)
						return false;

					if (Is4Node(current))
					{
						_slots[current].IsRed = true;
						_slots[slot.Left].IsRed = false;
						_slots[slot.Right].IsRed = false;

						if (parent != -1 && _slots[parent].IsRed)
							Balance(current, ref parent, grandParent, greatGrandParent);
					}
					greatGrandParent = grandParent;
					grandParent = parent;
					parent = current;
					current = order < 0 ? _slots[current].Left : _slots[current].Right;

					if (current != -1)
						continue;

					Debug.Assert(parent != -1, "Parent node can't be null here.");

					int newSlot = TakeSlot();
					_slots[newSlot] = new Slot(input, output, true);

					if (order < 0)
						_slots[parent].Left = newSlot;
					else
						_slots[parent].Right = newSlot;

					if (_slots[parent].IsRed)
						Balance(newSlot, ref parent, grandParent, greatGrandParent);

					return true;
				}
			}
			finally { _slots[_rootIndex].IsRed = false; }
		}
		public bool Remove(TInput input) => Remove(input, out _);
		public bool Remove(TInput input, out TOutput output)
		{
			output = default;
			if (_rootIndex == -1)
				return false;

			// Search for a node and then find its succesor. 
			// Then copy the item from the succesor to the matching node and delete the successor. 
			// If a node doesn't have a successor, we can replace it with its left child (if not empty.) 
			// or delete the matching node.
			// 
			// In top-down implementation, it is important to make sure the node to be deleted is not a 2-node.
			// Following code will make sure the node on the path is not a 2 Node. 

			//even if we don't actually remove from the set, we may be altering its structure (by doing rotations
			//and such). so update version to disable any enumerators/subsets working on it

			int current = _rootIndex;
			int parent = -1;
			int grandParent = -1;
			int match = -1;
			int parentOfMatch = -1;
			bool foundMatch = false;

			while (current != -1)
			{
				if (Is2Node(current))
				{ 
					// fix up 2-Node
					if (parent == -1)
						_slots[current].IsRed = true; // current is root. Mark it as red
					else
					{
						int sibling = GetSibling(current, parent);
						if (_slots[sibling].IsRed)
						{
							// If parent is a 3-node, flip the orientation of the red link. 
							// We can acheive this by a single rotation        
							// This case is converted to one of other cased below.
							Slot parentSlot = _slots[parent];
							Debug.Assert(parentSlot.IsBlack, "parent must be a black node!");
							
							if (parentSlot.Right == sibling)
								RotateLeft(parent);
							else
								RotateRight(parent);

							_slots[parent].IsRed = true;
							_slots[sibling].IsRed = false;    // parent's color
													  // sibling becomes child of grandParent or root after rotation. Update link from grandParent or root
							ReplaceChild(grandParent, parent, sibling);
							// sibling will become grandParent of current node 
							grandParent = sibling;
							if (parent == match)
								parentOfMatch = sibling;

							// update sibling, this is necessary for following processing
							sibling = GetSibling(current, parent);
						}
						Debug.Assert(sibling != -1 || _slots[sibling].IsBlack, "sibling must not be null and it must be black!");

						if (Is2Node(sibling))
						{
							//Merge2Nodes(parent, current, sibling)
							Debug.Assert(_slots[parent].IsRed, "parent must be be red");
							// combing two 2-nodes into a 4-node
							_slots[parent].IsRed = false;
							_slots[current].IsRed = true;
							_slots[sibling].IsRed = true;
						}
						else
						{
							// current is a 2-node and sibling is either a 3-node or a 4-node.
							// We can change the color of current to red by some rotation.
							RotationDirection rotation = RotationNeeded(parent, current, sibling);
							int newGrandParent = -1;
							Slot siblingSlot = _slots[sibling];
							Slot parentSlot = _slots[parent];
							switch (rotation)
							{
								case RotationDirection.Right:
									Debug.Assert(_slots[parent].Left == sibling, "sibling must be left child of parent!");
									Debug.Assert(_slots[siblingSlot.Left].IsRed, "Left child of sibling must be red!");
									_slots[siblingSlot.Left].IsRed = false;
									newGrandParent = parentSlot.Left;
									RotateRight(parent);
									break;
								case RotationDirection.Left:
									Debug.Assert(_slots[parent].Right == sibling, "sibling must be left child of parent!");
									Debug.Assert(_slots[siblingSlot.Right].IsRed, "Right child of sibling must be red!");
									_slots[siblingSlot.Right].IsRed = false;
									newGrandParent = parentSlot.Right;
									RotateLeft(parent);
									break;
								case RotationDirection.RightLeft:
									Debug.Assert(_slots[parent].Right == sibling, "sibling must be left child of parent!");
									Debug.Assert(_slots[siblingSlot.Left].IsRed, "Left child of sibling must be red!");
									newGrandParent = _slots[parentSlot.Right].Left;
									RotateRightLeft(parent);
									break;
								case RotationDirection.LeftRight:
									Debug.Assert(_slots[parent].Left == sibling, "sibling must be left child of parent!");
									Debug.Assert(_slots[siblingSlot.Right].IsRed, "Right child of sibling must be red!");
									newGrandParent = _slots[parentSlot.Left].Right;
									RotateLeftRight(parent);
									break;
							}

							_slots[newGrandParent].IsRed = parentSlot.IsRed;
							_slots[parent].IsRed = false;
							_slots[current].IsRed = true;
							ReplaceChild(grandParent, parent, newGrandParent);
							if (parent == match)
								parentOfMatch = newGrandParent;
						}
					}
				}

				// we don't need to compare any more once we found the match
				int order = foundMatch ? -1 : _inputComparer.Compare(input, _slots[current].Input);
				if (order == 0)
				{
					foundMatch = true;
					match = current;
					parentOfMatch = parent;
				}

				grandParent = parent;
				parent = current;

				if (order < 0)
					current = _slots[current].Left;
				else
					current = _slots[current].Right;       // continue the search in  right sub tree after we find a match
			}

			// move successor to the matching node position and replace links
			if (match != -1)
			{
				//ReplaceNode(match, parentOfMatch, parent, grandParent);
				//Replace the matching node with its succesor.
				//private void ReplaceNode(int match, int parentOfMatch, int succesor, int parentOfSuccesor)
				int succesor = parent;
				int parentOfSuccesor = grandParent;
				if (succesor == match)
				{
					// this node has no successor, should only happen if right child of matching node is null.
					Debug.Assert(_slots[match].Right == -1, "Right child must be null!");
					succesor = _slots[match].Left;
				}
				else
				{
					Debug.Assert(parentOfSuccesor != -1, "parent of successor cannot be null!");
					Slot succesorSlot = _slots[succesor];
					Debug.Assert(succesorSlot.Left == -1, "Left child of succesor must be null!");
					Debug.Assert
					(
						(succesorSlot.Right == -1 && succesorSlot.IsRed) || (_slots[succesorSlot.Right].IsRed && succesorSlot.IsBlack),
						"Succesor must be in valid state"
					);
					if (succesorSlot.Right != -1)
						_slots[succesorSlot.Right].IsRed = false;

					if (parentOfSuccesor != match)
					{
						// detach succesor from its parent and set its right child
						_slots[parentOfSuccesor].Left = succesorSlot.Right;
						_slots[succesor].Right = _slots[match].Right;
					}

					_slots[succesor].Left = _slots[match].Left;
				}

				if (succesor != -1)
					_slots[succesor].IsRed = _slots[match].IsRed;

				ReplaceChild(parentOfMatch, match, succesor);
				output = _slots[match].Output;
				_slots[match] = default;
				_slots[match].Left = _freeListIndex;
				_freeListIndex = match;
				_count--;
			}

			if (_rootIndex != -1)
				_slots[_rootIndex].IsRed = false;
			return foundMatch;
		}
		public bool TryGetValue(TInput input, out TOutput output)
		{
			int current = _rootIndex;
			for (; current != -1; )
			{
				int order = _inputComparer.Compare(input, _slots[current].Input);
				if(order == 0)
				{
					output = _slots[current].Output;
					return true;
				}
				current = order < 0 ? _slots[current].Left : _slots[current].Right;
			}
			output = default;
			return false;
		}
		public bool TryGetMin(TInput includedLowerBound, out TOutput output)
		{
			int current = _rootIndex;
			int foundMin = -1;
			for (; current != -1;)
			{
				int order = _inputComparer.Compare(includedLowerBound, _slots[current].Input);
				if (order == 0)
				{
					output = _slots[current].Output;
					return true;
				}
				if (order < 0)
					foundMin = current;
				current = order < 0 ? _slots[current].Left : _slots[current].Right;
			}
			if(foundMin != -1)
			{
				output = _slots[foundMin].Output;
				return true;
			}
			output = default;
			return false;
		}
		public void Clear()
		{
			_rootIndex = -1;
			_freeListIndex = -1;
			_usedSlotsCount = 0;
			_count = 0;
		}

		private int GetSibling(int node, int parent)
		{
			Slot parentSlot = _slots[parent];
			return _slots[parent].Left == node ? parentSlot.Right : parentSlot.Left;
		}
		private bool Is4Node(int current)
		{
			Slot currentSlot = _slots[current];
			bool isLeftRed = currentSlot.Left != -1 && _slots[currentSlot.Left].IsRed;
			bool isRightRed = currentSlot.Right != -1 && _slots[currentSlot.Right].IsRed;
			return isLeftRed && isRightRed;
		}
		private bool Is2Node(int current)
		{
			Slot currentSlot = _slots[current];
			bool isLeftNullOrBlack = currentSlot.Left == -1 || _slots[currentSlot.Left].IsBlack;
			bool isRightNullOrBlack = currentSlot.Right == -1 || _slots[currentSlot.Right].IsBlack;
			return currentSlot.IsBlack && isLeftNullOrBlack && isRightNullOrBlack;
		}
		private RotationDirection GetRotationDirection(int current, int parent, int grandParent)
		{
			//{parentIsOnRight, currentIsOnRight}
			int parentIsOnRight = _slots[grandParent].Right == parent ? 1 : 0;
			int currentIsOnRight = _slots[parent].Right == current ? 1 : 0;
			return (RotationDirection)((parentIsOnRight << 1) | currentIsOnRight);
		}
		private RotationDirection RotationNeeded(int parent, int current, int sibling)
		{
			Slot siblingSlot = _slots[sibling];
			bool isLeftSiblingSlotRed = siblingSlot.Left != -1 && _slots[siblingSlot.Left].IsRed;
			bool isRightSiblingSlotRed = siblingSlot.Right != -1 && _slots[siblingSlot.Right].IsRed;
			Debug.Assert(isLeftSiblingSlotRed || isRightSiblingSlotRed, "sibling must have at least one red child"); ;

			bool currentIsOnLeft = _slots[parent].Left == current;
			return isLeftSiblingSlotRed
					? (currentIsOnLeft ? RotationDirection.RightLeft : RotationDirection.Right)
					: (currentIsOnLeft ? RotationDirection.Left : RotationDirection.LeftRight);
		}
		private void Balance(int current, ref int parent, int grandParent, int greatGrandParent)
		{
			RotationDirection rotationDirection = GetRotationDirection(current, parent, grandParent);
			int nodeRoot = Rotate(grandParent, rotationDirection);
			ReplaceChild(greatGrandParent, grandParent, nodeRoot);
			parent = greatGrandParent;
		}
		private int Rotate(int current, RotationDirection rotationDirection)
		{
			int newRoot;
			Slot currentSlot = _slots[current];
			switch (rotationDirection)
			{
				case RotationDirection.Left:
				{
					newRoot = currentSlot.Right;
					RotateLeft(current);
					break;
				}
				case RotationDirection.Right:
				{
					newRoot = currentSlot.Left;
					RotateRight(current);
					break;
				}
				case RotationDirection.LeftRight:
				{
					newRoot = _slots[currentSlot.Left].Right;
					RotateLeftRight(current);
					break;
				}
				case RotationDirection.RightLeft:
				{
					newRoot = _slots[currentSlot.Right].Left;
					RotateRightLeft(current);
					break;
				}
				default: throw new ArgumentOutOfRangeException(nameof(rotationDirection));
			}
			_slots[current].IsRed = true;
			_slots[newRoot].IsRed = false;

			return newRoot;
		}
		private void RotateLeft(int current)
		{
			int child = _slots[current].Right;
			_slots[current].Right = _slots[child].Left;
			_slots[child].Left = current;
		}
		private void RotateLeftRight(int current)
		{
			int child = _slots[current].Left;
			int grandChild = _slots[child].Right;

			_slots[current].Left = _slots[grandChild].Right;
			_slots[grandChild].Right = current;
			_slots[child].Right = _slots[grandChild].Left;
			_slots[grandChild].Left = child;
		}
		private void RotateRight(int current)
		{
			int child = _slots[current].Left;
			_slots[current].Left = _slots[child].Right;
			_slots[child].Right = current;
		}
		private void RotateRightLeft(int current)
		{
			int child = _slots[current].Right;
			int grandChild = _slots[child].Left;

			_slots[current].Right = _slots[grandChild].Left;
			_slots[grandChild].Left = current;
			_slots[child].Left = _slots[grandChild].Right;
			_slots[grandChild].Right = child;
		}
		private void ReplaceChild(int current, int child, int newChild)
		{
			if (current == -1)
			{
				_rootIndex = newChild;
				return;
			}
			if (_slots[current].Left == child)
				_slots[current].Left = newChild;
			else
				_slots[current].Right = newChild;
		}

		private int TakeSlot()
		{
			int index;
			if (_freeListIndex != -1)
			{
				index = _freeListIndex;
				_freeListIndex = _slots[index].Left;
			}
			else
			{
				if (_usedSlotsCount == _slots.Length)
					ArrayExtensions.IncreaseLength(ref _slots);
				index = _usedSlotsCount++;
			}
			_count++;
			return index;
		}
	}
}
