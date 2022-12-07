![datis2](https://user-images.githubusercontent.com/34892440/205399639-3c2c4159-79cf-4c55-93fe-bc4d699db12f.gif)

# What is this? 
A likely-poorly-architected desktop app to continuously (well, once per minute) fetch and display FAA D-ATISs (thanks to the https://datis.clowd.io API) with a little bit of parsing.

This was a learning project as a first attempt to work with:
* C# and its async/await pattern
* WinUI 3 -- the latest and greatest in a long line of Windows UIs (and as such, bugs, lack of examples, etc.)
* Model-View-ViewModel design pattern
* XAML and x:Bind and INotifyPropertyChanged and making sure that changes to bound observable variables happen on the UI thread using Dispatch Queues
* The list goes on...I wouldn't have started if I knew how much I'd have to go look up...

# How do I get it?
TBD -- Microsoft makes it hard to distribute packages/executables these days. But you can always build from source using Visual Studio.

But it's probably easier to go to https://datis.clowd.io and get the same thing in web format.

# License
**MIT License**

Copyright 2022 Ken Greim

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
