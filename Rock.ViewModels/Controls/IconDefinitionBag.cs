// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.Collections.Generic;

namespace Rock.ViewModels.Controls
{
    public class IconDefinitionBag
    {
        /// <summary>
        /// The name of the icon
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A list of search terms that can be used to find this icon.
        /// </summary>
        public List<string> SearchTerms { get; set; } = new List<string>();

        /// <summary>
        /// A CSS class added to the icon element to cause the correct icon to display.
        /// </summary>
        public string StyleClass { get; set; }

        /// <summary>
        /// The SVG markup for the icon for preview purposes in the icon picker.
        /// </summary>
        public string IconSvg { get; set; }
    }
}
