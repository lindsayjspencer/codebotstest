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
import { action } from 'mobx';
import Cookies from 'js-cookie';
import Auth from "./Components/Auth/Auth";
import AllUsersPage from './Pages/Admin/AllUsersPage';
import AdminPage from './Pages/Admin/AdminPage';
import Topbar from "./Components/Topbar/Topbar";
import PageLinks from './Pages/Admin/PageLinks';
import { Redirect, Route, RouteComponentProps, Switch } from 'react-router';
import { SERVER_URL } from "../Constants";
import { store } from "Models/Store";

// This ts-ignore is needed since there is no types for graphiql
// @ts-ignore
import GraphiQL from 'graphiql';
// % protected region % [Add any extra imports here] off begin
// % protected region % [Add any extra imports here] end

export default class Admin extends React.Component<RouteComponentProps> {
	@action
	private setAppLocation = () => {
		store.appLocation = 'admin';
	}

	public componentDidMount() {
		this.setAppLocation();
	}

	public render() {
		return (
			<>
				<div className="body-container">
					{
					// % protected region % [Modify Topbar] off begin
					}
					<Topbar currentLocation="admin" />
					{
					// % protected region % [Modify Topbar] end
					}
					<div className="admin">
						<Auth {...this.props}>
							<this.adminSwitch />
						</Auth>
					</div>
				</div>
			</>
		);
	}

	private adminSwitch = () => {
		if (!store.userGroups.some(ug => ug.hasBackendAccess)) {
			return <Redirect to="/404" />;
		}

		const path = this.props.match.path === '/' ? '' : this.props.match.path;

		const graphiQl = () => (
			<div className="graphiql-content-container body-content">
				<GraphiQL fetcher={this.graphiQLFetcher} />
			</div>
		);

		return (
			<>
				{
				// % protected region % [Override contents here] off begin
				}
				<PageLinks {...this.props} />
				{
				// % protected region % [Override contents here] end
				}
				<div className="body-content">
					<Switch>
						{/* These routes require a login to view */}

						{/* Admin entity pages */}
						<Route exact={true} path={`${path}`} component={AdminPage} />
						<Route path={`${path}/User`} component={AllUsersPage} />

						{
						// % protected region % [Add any extra page routes here] off begin
						}
						{
						// % protected region % [Add any extra page routes here] end
						}
					</Switch>
				</div>
				{
				// % protected region % [Add any admin footer content here] off begin
				}
				{
				// % protected region % [Add any admin footer content here] end
				}

				<Switch>
					<Route path={`${path}/graphiql`} component={graphiQl} />
				</Switch>
			</>
		);
	}

	private graphiQLFetcher = (graphQLParams: {}) => {
		const token = Cookies.get('XSRF-TOKEN');
		return fetch(`${SERVER_URL}/api/graphql`, {
			method: 'post',
			headers: {
				'Content-Type': 'application/json',
				'X-XSRF-TOKEN': token ? token : '',
			},
			body: JSON.stringify(graphQLParams),
		}).then(response => response.json());
	}
}