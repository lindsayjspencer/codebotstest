/*
 * @bot-written
 * 
 * WARNING AND NOTICE
 * Any access, download, storage, and/or use of this source code is subject to the terms and conditions of the
 * Full Software Licence as accepted by you before being granted access to this source code and other materials,
 * the terms of which can be accessed on the Codebots website at https://codebots.com/full-software-licence. Any
 * commercial use in contravention of the terms of the Full Software Licence may be pursued by Codebots through
 * licence termination and further legal action, and be required to indemnify Codebots for any loss or damage,
 * including interest and costs. You are deemed to have accepted the terms of the Full Software Licence on any
 * access, download, storage, and/or use of this source code.
 * 
 * BOT WARNING
 * This file is bot-written.
 * Any changes out side of "protected regions" will be lost next time the bot makes any changes.
 */
import * as React from 'react';
import { observer } from 'mobx-react';
import { Redirect, RouteComponentProps } from 'react-router';
import { PageWrapper } from 'Views/Components/PageWrapper/PageWrapper';
import { store } from '../../../Models/Store';
// % protected region % [Add any extra imports here] off begin
// % protected region % [Add any extra imports here] end

@observer
export default class AdminPage extends React.Component<RouteComponentProps> {

	public render() {
		// % protected region % [Override contents here] off begin
		return (
			<PageWrapper {...this.props}>
				<div>
					<h2>Admin Section</h2>
				</div>
			</PageWrapper>
		);
		// % protected region % [Override contents here] end
	}
}
