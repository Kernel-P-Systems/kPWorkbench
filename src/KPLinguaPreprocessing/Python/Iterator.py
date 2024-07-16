

class ExecuteIterator:

    def __init__(self, iterators, restrictions):
        self.iterators = iterators
        self.restrictions = restrictions

    def initAll(self):
        for iterator in self.iterators:
            iterator.init()

    def hasNext(self):
        b = False
        index = 0
        while index < len(self.iterators) and not b:
            b = self.iterators[index].hasNext()
            if not b:
                index += 1
        return b

    def next(self):
        b = False
        index = 0
        while index < len(self.iterators) and not b:
            b = self.iterators[index].hasNext()
            if b:
                self.iterators[index].next()
                if index > 0:
                    for i in reversed(range(index)):
                        self.iterators[i].init()
            else:
                index += 1

    def isValid(self):
        return self.restrictions.evaluate()

    def execute(self):
        print(self.iterators)
        while self.hasNext():
            self.next()
            print(self.iterators)
