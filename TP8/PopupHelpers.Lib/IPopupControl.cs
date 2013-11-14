using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialEbola.Lib.PopupHelpers
{
	public interface IPopupControl
	{
		void SetParent(PopupHelper parent);
		void Closed(CloseAction closeAction);
		void Opened();
	}
}
