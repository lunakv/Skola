using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NezarkaDotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new NezarkaServer(Console.In, Console.Out);
            server.ProcessRequests();
        }
    }

    class ViewStore
    {
        TextWriter _output;
        ModelStore _model;

        public ViewStore(TextWriter output, ModelStore model)
        {
            _output = output;
            _model = model;
        }

        public void ShowInvalidRequest()
        {
            _output.WriteLine("<!DOCTYPE html>");
            _output.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
            _output.WriteLine("<head>");
            _output.WriteLine("	<meta charset=\"utf-8\" />");
            _output.WriteLine("	<title>Nezarka.net: Online Shopping for Books</title>");
            _output.WriteLine("</head>");
            _output.WriteLine("<body>");
            _output.WriteLine("<p>Invalid request.</p>");
            _output.WriteLine("</body>");
            _output.WriteLine("</html>");
            _output.WriteLine("====");
        }

        private void PrintCommonHeader(Customer cust)
        {
            _output.WriteLine("<!DOCTYPE html>");
            _output.WriteLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
            _output.WriteLine("<head>");
            _output.WriteLine("	<meta charset=\"utf-8\" />");
            _output.WriteLine("	<title>Nezarka.net: Online Shopping for Books</title>");
            _output.WriteLine("</head>");
            _output.WriteLine("<body>");
            _output.WriteLine("	<style type=\"text/css\">");
            _output.WriteLine("		table, th, td {");
            _output.WriteLine("			border: 1px solid black;");
            _output.WriteLine("			border-collapse: collapse;");
            _output.WriteLine("		}");
            _output.WriteLine("		table {");
            _output.WriteLine("			margin-bottom: 10px;");
            _output.WriteLine("		}");
            _output.WriteLine("		pre {");
            _output.WriteLine("			line-height: 70%;");
            _output.WriteLine("		}");
            _output.WriteLine("	</style>");
            _output.WriteLine("	<h1><pre>  v,<br />Nezarka.NET: Online Shopping for Books</pre></h1>");
            _output.WriteLine("	{0}, here is your menu:", cust.FirstName);
            _output.WriteLine("	<table>");
            _output.WriteLine("		<tr>");
            _output.WriteLine("			<td><a href=\"/Books\">Books</a></td>");
            _output.WriteLine("			<td><a href=\"/ShoppingCart\">Cart ({0})</a></td>", cust.ShoppingCart.Items.Count);
            _output.WriteLine("		</tr>");
            _output.WriteLine("	</table>");
        }

        public void ShowAllBooks(Customer cust)
        {
            PrintCommonHeader(cust);
            _output.WriteLine("	Our books for you:");
            _output.WriteLine("	<table>");
            IList<Book> books = _model.GetBooks();

            for (int i = 0; i < books?.Count; i++)
            {
                Book currentBook = books[i];
                if (i % 3 == 0)
                    _output.WriteLine("		<tr>");
                _output.WriteLine("			<td style=\"padding: 10px;\">");
                _output.WriteLine("				<a href=\"/Books/Detail/{0}\">{1}</a><br />", currentBook.Id, currentBook.Title);
                _output.WriteLine("				Author: {0}<br />", currentBook.Author);
                _output.WriteLine("				Price: {0} EUR &lt;<a href=\"/ShoppingCart/Add/{1}\">Buy</a>&gt;", currentBook.Price, currentBook.Id);
                _output.WriteLine("			</td>");
                if (i % 3 == 2 || i == books.Count - 1)
                    _output.WriteLine("		</tr>");
            }

            _output.WriteLine("	</table>");
            _output.WriteLine("</body>");
            _output.WriteLine("</html>");
            _output.WriteLine("====");
        }

        public void ShowShoppingCart(Customer cust)
        {
            PrintCommonHeader(cust);
            
            if (cust.ShoppingCart.Items.Count == 0)
            {
                ShowEmptyCart();
                return;
            }

            _output.WriteLine("	Your shopping cart:");
            _output.WriteLine("	<table>");
            _output.WriteLine("		<tr>");
            _output.WriteLine("			<th>Title</th>");
            _output.WriteLine("			<th>Count</th>");
            _output.WriteLine("			<th>Price</th>");
            _output.WriteLine("			<th>Actions</th>");
            _output.WriteLine("		</tr>");
            
            int totalCost = 0;
            for (int i = 0; i < cust.ShoppingCart.Items.Count; i++)
            {
                ShoppingCartItem currentItem = cust.ShoppingCart.Items[i];
                Book currentBook = _model.GetBook(currentItem.BookId);
                int itemPrice = currentItem.Count * currentBook.Price;
                totalCost += itemPrice;

                _output.WriteLine("		<tr>");
                _output.WriteLine("			<td><a href=\"/Books/Detail/{0}\">{1}</a></td>", currentBook.Id, currentBook.Title);
                _output.WriteLine("			<td>{0}</td>", currentItem.Count);
                if (currentItem.Count == 1)
                    _output.WriteLine("			<td>{0} EUR</td>", currentBook.Price);
                else
                    _output.WriteLine("			<td>{0} * {1} = {2} EUR</td>", currentItem.Count, currentBook.Price, itemPrice);
                _output.WriteLine("			<td>&lt;<a href=\"/ShoppingCart/Remove/{0}\">Remove</a>&gt;</td>", currentBook.Id);
                _output.WriteLine("		</tr>");
            }

            _output.WriteLine("	</table>");
            _output.WriteLine("	Total price of all items: {0} EUR", totalCost);
            _output.WriteLine("</body>");
            _output.WriteLine("</html>");
            _output.WriteLine("====");
        }

        private void ShowEmptyCart()
        {
            _output.WriteLine("	Your shopping cart is EMPTY.");
            _output.WriteLine("</body>");
            _output.WriteLine("</html>");
            _output.WriteLine("====");
        }

        public void ShowBookDetail(Customer cust, Book book)
        {
            PrintCommonHeader(cust);

            _output.WriteLine("	Book details:");
            _output.WriteLine("	<h2>{0}</h2>", book.Title);
            _output.WriteLine("	<p style=\"margin-left: 20px\">");
            _output.WriteLine("	Author: {0}<br />", book.Author);
            _output.WriteLine("	Price: {0} EUR<br />", book.Price);
            _output.WriteLine("	</p>");
            _output.WriteLine("	<h3>&lt;<a href=\"/ShoppingCart/Add/{0}\">Buy this book</a>&gt;</h3>", book.Id);
            _output.WriteLine("</body>");
            _output.WriteLine("</html>");
            _output.WriteLine("====");
        }
    }

    class RequestParser
    {
        private string _domainString;
        private int domainLength;

        public RequestParser(string domain)
        {
            _domainString = domain;
            domainLength = domain.Length;
        }

        public string GetRequestType(string request)
        {
            return request.Split(' ')[0];
        }

        public int GetCustID(string request)
        {
            string[] splitRequest = request.Split(' ');
            if (splitRequest.Length < 2) return -1;

            bool valid = int.TryParse(splitRequest[1], out int result);
            return valid ? result : -1;
        }

        public string GetCommand(string request)
        {
            string[] splitRequest = request.Split(' ');
            if (splitRequest.Length < 3) return null;

            string fullAdress = splitRequest[2];
            if (fullAdress.Length < domainLength) return null;

            string domain = fullAdress.Substring(0, domainLength);
            if (domain != _domainString) return null;

            return fullAdress.Substring(domainLength);
        }

        public int GetBookID(string command)
        {
            var split = command.Split('/');
            bool valid = int.TryParse(split[split.Length-1], out int result);
            return valid ? result : -1;
        }
    }

    class ControllerStore
    {
        private ModelStore _model;
        private ViewStore _view;
        private RequestParser _parser = new RequestParser("http://www.nezarka.net/");

        public ControllerStore(ModelStore model, ViewStore view)
        {
            _model = model;
            _view = view;
        }

        public void ProcessRequest(string request)
        {
            if (_parser.GetRequestType(request) != "GET")
            {
                _view.ShowInvalidRequest();
                return;
            }

            int custID = _parser.GetCustID(request);
            Customer cust = _model.GetCustomer(custID);
            if (cust == null)
            {
                _view.ShowInvalidRequest();
                return;
            }

            string command = _parser.GetCommand(request);
            if (command == null)
            {
                _view.ShowInvalidRequest();
                return;
            }

            if (command == "Books")
                _view.ShowAllBooks(cust);            
            else if (command == "ShoppingCart")
                _view.ShowShoppingCart(cust);
            else
            {
                int bookID = _parser.GetBookID(command);
                Book book = _model.GetBook(bookID);
                if (book == null)
                {
                    _view.ShowInvalidRequest();
                    return;
                }

                if (command.Contains("Books/Detail/"))
                {
                    _view.ShowBookDetail(cust, book);
                }
                else if (command.Contains("ShoppingCart/Add/"))
                {
                    AddBookToCart(cust, bookID);
                }
                else if (command.Contains("ShoppingCart/Remove/"))
                {
                    RemoveBookFromCart(cust, bookID);
                }
                else
                    _view.ShowInvalidRequest();
            }
            
        }

        private void AddBookToCart(Customer cust, int bookID)
        {
            ShoppingCartItem modifiedItem = cust.ShoppingCart.GetItem(bookID);
            if (modifiedItem == null)
            {
                cust.ShoppingCart.Items.Add(new ShoppingCartItem {BookId = bookID, Count = 1});
            }
            else
                modifiedItem.Count++;

            _view.ShowShoppingCart(cust);
        }

        private void RemoveBookFromCart(Customer cust, int bookID)
        {
            ShoppingCartItem modifiedItem = cust.ShoppingCart.GetItem(bookID);
            if (modifiedItem == null)
            {
                _view.ShowInvalidRequest();
                return;
            }

            modifiedItem.Count--;
            if (modifiedItem.Count == 0)
            {
                cust.ShoppingCart.Items.Remove(modifiedItem);
            }

            _view.ShowShoppingCart(cust);            
        }

        
    }

    class NezarkaServer : IServer
    {
        private ControllerStore _controller;
        private ModelStore _model;
        private ViewStore _view;
        private TextReader _requestReader;

        public NezarkaServer(TextReader input, TextWriter output)
        {
            _requestReader = input;
            _model = ModelStore.LoadFrom(input);
            _view = new ViewStore(output, _model);            
            _controller = new ControllerStore(_model, _view);
        }

        private void FinishProcessing()
        {
            
        }

        public void ProcessRequests()
        {
            if (_model == null)
            {
                Console.Write("Data error.");
                return;
            }
            string request;
            while ((request = _requestReader.ReadLine()) != null)
            {
                _controller.ProcessRequest(request);
            }
        }
    }



    interface IServer
    {
        void ProcessRequests();
    }



}
