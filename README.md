# XMLReader
A super simple XML reader - all it does is fetch a feed and loop through the elements using .NET XmlDocument. It does fetch using a header request, which some sites require such as the US Weather feed.

The current weather XML feed is very small and fast, so we simply have it run on the main thread. This is not a good idea for most XML since the feed can be slow and the main thread has to wait before moving on. The test scene has a rotating cube which makes this obvious if you try to fetch the forecast from the main thread.

To solve this, we open a new thread just for the XML reading. It is implemented as simply as possible, and for this use I have switched to the XDocument, which offers a bit more in the way of shortcuts for parsing XML.

MIT License

Copyright (c) 2017 Bryan Leister

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
