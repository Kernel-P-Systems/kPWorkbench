using KPLinguaPreprocessing.Models;
using System;
using System.Collections.Generic;

namespace KPLinguaPreprocessing
{
    public class ExecuteIterator
    {
        private readonly List<Iterator> iterators;
        private readonly Base restrictions;

        public ExecuteIterator(List<Iterator> iterators, Base restrictions)
        {
            this.iterators = iterators;
            this.restrictions = restrictions;
        }

        public void InitAll()
        {
            foreach (var iterator in iterators)
            {
                iterator.Init();
            }
        }

        public bool HasNext()
        {
            bool hasNext = false;
            int index = 0;
            while (index < iterators.Count && !hasNext)
            {
                hasNext = iterators[index].HasNext();
                if (!hasNext)
                {
                    index++;
                }
            }
            return hasNext;
        }

        public void Next()
        {
            bool hasNext = false;
            int index = 0;
            while (index < iterators.Count && !hasNext)
            {
                hasNext = iterators[index].HasNext();
                if (hasNext)
                {
                    iterators[index].Next();
                    if (index > 0)
                    {
                        for (int i = index - 1; i >= 0; i--)
                        {
                            iterators[i].Init();
                        }
                    }
                }
                else
                {
                    index++;
                }
            }
        }

        public bool IsValid()
        {
            return restrictions.Evaluate() != 0;
        }

        public void Execute()
        {
            Console.WriteLine(string.Join(", ", iterators));
            while (HasNext())
            {
                Next();
                Console.WriteLine(string.Join(", ", iterators));
            }
        }
    }
}
