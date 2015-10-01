using UnityEngine;
using System.Collections;

namespace Infinario.Commands
{
	internal interface Command
	{
		object Execute();
	}
}
