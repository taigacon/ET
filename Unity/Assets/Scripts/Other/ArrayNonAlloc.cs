using System;

namespace BK
{
	public static class ArrayNonAlloc<T>
	{
		private static T[] array1 = new T[1];
		private static T[] array2 = new T[2];
		private static T[] array3 = new T[3];
		private static T[] array4 = new T[4];
		private static T[] array5 = new T[5];
		private static T[] array6 = new T[6];
		private static T[] array7 = new T[7];
		private static T[] array8 = new T[8];
		private static T[] array9 = new T[9];
		private static T[] array10 = new T[10];
		private static ArrayNonAllocHandler<T> array1Handler = new ArrayNonAllocHandler<T>(array1);
		private static ArrayNonAllocHandler<T> array2Handler = new ArrayNonAllocHandler<T>(array2);
		private static ArrayNonAllocHandler<T> array3Handler = new ArrayNonAllocHandler<T>(array3);
		private static ArrayNonAllocHandler<T> array4Handler = new ArrayNonAllocHandler<T>(array4);
		private static ArrayNonAllocHandler<T> array5Handler = new ArrayNonAllocHandler<T>(array5);
		private static ArrayNonAllocHandler<T> array6Handler = new ArrayNonAllocHandler<T>(array6);
		private static ArrayNonAllocHandler<T> array7Handler = new ArrayNonAllocHandler<T>(array7);
		private static ArrayNonAllocHandler<T> array8Handler = new ArrayNonAllocHandler<T>(array8);
		private static ArrayNonAllocHandler<T> array9Handler = new ArrayNonAllocHandler<T>(array9);
		private static ArrayNonAllocHandler<T> array10Handler = new ArrayNonAllocHandler<T>(array10);

		public static ArrayNonAllocHandler<T> Use(T t1)
		{
			array1[0] = t1;
			return array1Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2)
		{
			array2[0] = t1;
			array2[1] = t2;
			return array2Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2, T t3)
		{
			array3[0] = t1;
			array3[1] = t2;
			array3[2] = t3;
			return array3Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2, T t3, T t4)
		{
			array4[0] = t1;
			array4[1] = t2;
			array4[2] = t3;
			array4[3] = t4;
			return array4Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2, T t3, T t4, T t5)
		{
			array5[0] = t1;
			array5[1] = t2;
			array5[2] = t3;
			array5[3] = t4;
			array5[4] = t5;
			return array5Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2, T t3, T t4, T t5, T t6)
		{
			array6[0] = t1;
			array6[1] = t2;
			array6[2] = t3;
			array6[3] = t4;
			array6[4] = t5;
			array6[5] = t6;
			return array6Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2, T t3, T t4, T t5, T t6, T t7)
		{
			array7[0] = t1;
			array7[1] = t2;
			array7[2] = t3;
			array7[3] = t4;
			array7[4] = t5;
			array7[5] = t6;
			array7[6] = t7;
			return array7Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8)
		{
			array8[0] = t1;
			array8[1] = t2;
			array8[2] = t3;
			array8[3] = t4;
			array8[4] = t5;
			array8[5] = t6;
			array8[6] = t7;
			array8[7] = t8;
			return array8Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9)
		{
			array9[0] = t1;
			array9[1] = t2;
			array9[2] = t3;
			array9[3] = t4;
			array9[4] = t5;
			array9[5] = t6;
			array9[6] = t7;
			array9[7] = t8;
			array9[8] = t9;
			return array9Handler;
		}
		public static ArrayNonAllocHandler<T> Use(T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10)
		{
			array10[0] = t1;
			array10[1] = t2;
			array10[2] = t3;
			array10[3] = t4;
			array10[4] = t5;
			array10[5] = t6;
			array10[6] = t7;
			array10[7] = t8;
			array10[8] = t9;
			array10[9] = t10;
			return array10Handler;
		}
	}

	public class ArrayNonAllocHandler<T>: IDisposable
	{
		public T[] Array { get; }

		public ArrayNonAllocHandler(T[] array)
		{
			this.Array = array;
		}

		public void Dispose()
		{
			for (int i = 0; i < this.Array.Length; i++)
			{
				this.Array[i] = default(T);
			}
		}
	}
}